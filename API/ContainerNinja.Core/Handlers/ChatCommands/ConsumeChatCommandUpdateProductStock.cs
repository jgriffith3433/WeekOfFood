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
    [ChatCommandModel(new[] { "update_stocked_products" })]
    public class ConsumeChatCommandUpdateStockedProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateStockedProducts>
    {
        public ChatAICommandDTOUpdateStockedProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandUpdateProductStockHandler : IRequestHandler<ConsumeChatCommandUpdateStockedProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandUpdateProductStockHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUpdateStockedProducts model, CancellationToken cancellationToken)
        {
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
            var productStocksToUpdate = new List<ProductStock>();
            //first go through all the Ids we have and check if they all exist
            foreach (var item in model.Command.StockedProducts)
            {
                if (item.StockedProductId.HasValue)
                {
                    if (item.StockedProductId.Value == 1)
                    {
                        //TODO: ai magically saying 1
                        item.StockedProductId = null;
                        continue;
                    }
                    var existingProductStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == item.StockedProductId);
                    if (existingProductStockEntity == null)
                    {
                        throw new ChatAIException($"Could not find Product Stock by ID: {item.StockedProductId}", @"{ ""name"": ""get_stocked_product_id"" }");
                    }
                    if (item.Units != null)
                    {
                        existingProductStockEntity.Units = item.Units;
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
                            if (item.Units != null)
                            {
                                newProductStockEntity.Units = item.Units;
                            }
                        }
                        var newProductEntity = _repository.Products.CreateProxy();
                        {
                            newProductEntity.Name = item.StockedProductName;
                            newProductEntity.ProductStock = newProductStockEntity;
                        };
                        newProductStockEntity.Product = newProductEntity;
                        productStocksToAdd.Add(newProductStockEntity);
                    }
                    else if (existingProductStockEntities.Count == 1)
                    {
                        if (existingProductStockEntities[0].Name.ToLower() == item.StockedProductName.ToLower())
                        {
                            if (item.Units != null)
                            {
                                existingProductStockEntities[0].Units = item.Units;
                            }
                            productStocksToUpdate.Add(existingProductStockEntities[0]);
                        }
                        else
                        {
                            var systemMessage = $"Record '{existingProductStockEntities[0].Name}' found for '{item.StockedProductName}'. Is that the record you want to update?\n";

                            var productStockObject = new JObject();
                            productStockObject["StockedProductId"] = existingProductStockEntities[0].Id;
                            productStockObject["StockedProductName"] = existingProductStockEntities[0].Name;
                            productStockObject["WalmartId"] = existingProductStockEntities[0].Product.WalmartId;

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
                            if (item.Units != null)
                            {
                                exactMatch.Units = item.Units;
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
                                productStockObject["StockedProductName"] = existingProductStockEntity.Name;
                                productStockObject["WalmartId"] = existingProductStockEntity.Product.WalmartId;
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
                productStockObject["StockedProductName"] = productStock.Name;
                productStockObject["WalmartId"] = productStock.Product.WalmartId;
                productStockArray.Add(productStockObject);
            }
            model.Response.NavigateToPage = "product-stocks";
            return JsonConvert.SerializeObject(productStockArray);
        }
    }
}