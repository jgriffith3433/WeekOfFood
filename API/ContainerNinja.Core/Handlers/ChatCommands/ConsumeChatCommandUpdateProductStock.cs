using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using LinqKit;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "update_product_stock" })]
    public class ConsumeChatCommandUpdateProductStock : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateProductStock>
    {
        public ChatAICommandDTOUpdateProductStock Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandUpdateProductStockHandler : IRequestHandler<ConsumeChatCommandUpdateProductStock, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandUpdateProductStockHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUpdateProductStock model, CancellationToken cancellationToken)
        {
            var productStocksToUpdate = new List<ProductStock>();
            //first go through all the Ids we have and check if they all exist
            foreach (var item in model.Command.StockedProducts)
            {
                if (item.StockedProductId.HasValue)
                {
                    var existingProductStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == item.StockedProductId);
                    if (existingProductStockEntity == null)
                    {
                        throw new ChatAIException($"Could not find Product Stock by ID: {item.StockedProductId}", @"{ ""name"": ""get_stocked_product_id"" }");
                    }
                    if (item.Quantity != null)
                    {
                        existingProductStockEntity.Units = item.Quantity;
                    }
                    if (item.UnitType != null)
                    {
                        existingProductStockEntity.Product.UnitType = item.UnitType.UnitTypeFromString();
                    }
                    productStocksToUpdate.Add(existingProductStockEntity);
                }
            }
            //then go through items that don't have an Id and get the product stock by name
            //if there are multiple results, ask the user which one they want to take stock of
            //if there is one record but the name is not an exact match then verify
            //if there are none then create a new product stock record
            var productStocksToAdd = new List<ProductStock>();
            foreach (var item in model.Command.StockedProducts)
            {
                if (item.StockedProductId.HasValue == false)
                {
                    var predicate = PredicateBuilder.New<ProductStock>();
                    var searchTerms = string.Join(' ', item.StockedProductName.ToLower().Split('-')).Split(' ');
                    foreach (var searchTerm in searchTerms)
                    {
                        predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
                    }

                    var existingProductStockEntities = _repository.ProductStocks.Set.AsExpandable().Where(predicate).ToList();
                    if (existingProductStockEntities.Count == 0)
                    {
                        var newProductStockEntity = _repository.ProductStocks.CreateProxy();
                        {
                            newProductStockEntity.Name = item.StockedProductName;
                            if (item.Quantity != null)
                            {
                                newProductStockEntity.Units = item.Quantity;
                            }
                        }
                        var newProductEntity = _repository.Products.CreateProxy();
                        {
                            newProductEntity.Name = item.StockedProductName;
                            newProductEntity.ProductStock = newProductStockEntity;
                            if (item.UnitType != null)
                            {
                                newProductEntity.UnitType = item.UnitType.UnitTypeFromString();
                            }
                        };
                        newProductStockEntity.Product = newProductEntity;
                        productStocksToAdd.Add(newProductStockEntity);
                    }
                    else if (existingProductStockEntities.Count == 1)
                    {
                        if (existingProductStockEntities[0].Name.ToLower() == item.StockedProductName.ToLower())
                        {
                            if (item.Quantity != null)
                            {
                                existingProductStockEntities[0].Units = item.Quantity;
                            }
                            if (item.UnitType != null)
                            {
                                existingProductStockEntities[0].Product.UnitType = item.UnitType.UnitTypeFromString();
                            }
                            productStocksToUpdate.Add(existingProductStockEntities[0]);
                        }
                        else
                        {
                            var systemMessage = $"Record '{existingProductStockEntities[0].Name}' found for '{item.StockedProductName}'. Is that the record you want to update?\n";

                            var productStockObject = new JObject();
                            productStockObject["StockedProductId"] = existingProductStockEntities[0].Id;
                            productStockObject["ItemName"] = existingProductStockEntities[0].Name;

                            systemMessage += JsonConvert.SerializeObject(productStockObject);
                            throw new ChatAIException(systemMessage);
                        }
                    }
                    else
                    {
                        //TODO: Probably a way to one line this first block into a query
                        var exactMatch = existingProductStockEntities.FirstOrDefault(ps => ps.Name.ToLower() == item.StockedProductName.ToLower());

                        if (exactMatch != null)
                        {
                            //check if there are no "too similar" matches
                            //for instance: "trail mix" could be "raisin free trail mix" or "trail mix snack"
                            var tooSimilarMatch = existingProductStockEntities.FirstOrDefault(ps => ps != exactMatch && ps.Name.ToLower().Contains(item.StockedProductName.ToLower()));
                            if (tooSimilarMatch != null)
                            {
                                exactMatch = null;
                            }
                        }

                        if (exactMatch != null)
                        {
                            if (item.Quantity != null)
                            {
                                exactMatch.Units = item.Quantity;
                            }
                            if (item.UnitType != null)
                            {
                                exactMatch.Product.UnitType = item.UnitType.UnitTypeFromString();
                            }
                            productStocksToUpdate.Add(exactMatch);
                        }
                        else
                        {
                            var systemMessage = $"Multiple records found for '{item.StockedProductName}'. Did you mean to update any of these?\n";
                            var multipleRecordsArray = new JArray();
                            foreach (var existingProductStockEntity in existingProductStockEntities)
                            {
                                var productStockObject = new JObject();
                                productStockObject["StockedProductId"] = existingProductStockEntity.Id;
                                productStockObject["ItemName"] = existingProductStockEntity.Name;
                                multipleRecordsArray.Add(productStockObject);
                            }

                            systemMessage += JsonConvert.SerializeObject(multipleRecordsArray);
                            throw new ChatAIException(systemMessage);
                        }
                    }
                }
            }

            var takeProductStock = new List<ProductStock>();
            foreach (var newProductStock in productStocksToAdd)
            {
                _repository.ProductStocks.Add(newProductStock);
                takeProductStock.Add(newProductStock);
            }
            foreach (var existingProductStock in productStocksToUpdate)
            {
                _repository.ProductStocks.Update(existingProductStock);
                takeProductStock.Add(existingProductStock);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var productStockArray = new JArray();
            foreach (var productStock in takeProductStock)
            {
                var productStockObject = new JObject();
                productStockObject["StockedProductId"] = productStock.Id;
                productStockObject["ItemName"] = productStock.Name;
                productStockArray.Add(productStockObject);
            }
            return "Updated stock:\n" + JsonConvert.SerializeObject(productStockArray);
        }
    }
}