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
    [ChatCommandModel(new[] { "create_kitchen_products" })]
    public class ConsumeChatCommandCreateStockedProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateStockedProducts>
    {
        public ChatAICommandDTOCreateStockedProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateProductStockHandler : IRequestHandler<ConsumeChatCommandCreateStockedProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateProductStockHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateStockedProducts model, CancellationToken cancellationToken)
        {
            //then go through items that don't have an Id and get the product stock by name
            //if there are multiple results, ask the user which one they want to take stock of
            //if there is one record but the name is not an exact match then verify
            //if there are none then create a new product stock record
            var productStocksToAdd = new List<ProductStock>();
            var productStocksToUpdate = new List<ProductStock>();
            foreach (var item in model.Command.KitchenProducts)
            {
                var predicate = PredicateBuilder.New<ProductStock>();
                var searchTerms = string.Join(' ', item.KitchenProductName.ToLower().Split('-')).Split(' ');
                foreach (var searchTerm in searchTerms)
                {
                    predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
                    if (searchTerm[searchTerm.Length - 1] == 's')
                    {
                        predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm.Substring(0, searchTerm.Length - 1)));
                    }
                }

                var existingProductStockEntities = _repository.ProductStocks.Set.AsExpandable().Where(predicate).ToList();
                if (existingProductStockEntities.Count == 0)
                {
                    var newProductStockEntity = _repository.ProductStocks.CreateProxy();
                    {
                        newProductStockEntity.Name = item.KitchenProductName;
                        newProductStockEntity.Units = item.Units;
                        newProductStockEntity.UnitType = item.KitchenUnitType;
                    }
                    productStocksToAdd.Add(newProductStockEntity);
                }
                else if (existingProductStockEntities.Count == 1)
                {
                    if (existingProductStockEntities[0].Name.ToLower() == item.KitchenProductName.ToLower())
                    {
                        existingProductStockEntities[0].Units = item.Units;
                        existingProductStockEntities[0].UnitType = item.KitchenUnitType;
                        productStocksToUpdate.Add(existingProductStockEntities[0]);
                    }
                    else
                    {
                        var systemMessage = $"Record '{existingProductStockEntities[0].Name}' found for '{item.KitchenProductName}'. Is that the record you want to update?\n";

                        var productStockObject = new JObject();
                        productStockObject["KitchenProductId"] = existingProductStockEntities[0].Id;
                        productStockObject["KitchenProductName"] = existingProductStockEntities[0].Name;

                        systemMessage += JsonConvert.SerializeObject(productStockObject);
                        throw new ChatAIException(systemMessage, "none");
                    }
                }
                else
                {
                    //TODO: Probably a way to one line this first block into a query
                    var exactMatch = existingProductStockEntities.FirstOrDefault(ps => ps.Name.ToLower() == item.KitchenProductName.ToLower());

                    if (exactMatch != null)
                    {
                        //check if there are no "too similar" matches
                        //for instance: "trail mix" could be "raisin free trail mix" or "trail mix snack"
                        var tooSimilarMatch = existingProductStockEntities.FirstOrDefault(ps => ps != exactMatch && ps.Name.ToLower().Contains(item.KitchenProductName.ToLower()));
                        if (tooSimilarMatch != null)
                        {
                            exactMatch = null;
                        }
                    }

                    if (exactMatch != null)
                    {
                        exactMatch.Units = item.Units;
                        exactMatch.UnitType = item.KitchenUnitType;
                        productStocksToUpdate.Add(exactMatch);
                    }
                    else
                    {
                        var systemMessage = $"Multiple records found for '{item.KitchenProductName}'. Did you mean to update any of these?\n";
                        var multipleRecordsArray = new JArray();
                        foreach (var existingProductStockEntity in existingProductStockEntities)
                        {
                            var productStockObject = new JObject();
                            productStockObject["KitchenProductId"] = existingProductStockEntity.Id;
                            productStockObject["KitchenProductName"] = existingProductStockEntity.Name;
                            multipleRecordsArray.Add(productStockObject);
                        }

                        systemMessage += JsonConvert.SerializeObject(multipleRecordsArray);
                        throw new ChatAIException(systemMessage, "none");
                    }
                }
            }

            foreach (var newProductStock in productStocksToAdd)
            {
                _repository.ProductStocks.Add(newProductStock);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var inventoryModifiedObject = new JObject
            {
                { "Flow", "User is adding to their kitchen inventory" }
            };
            if (productStocksToAdd.Count > 0)
            {
                var addedRecordsObject = new JObject
                {
                    { "Message", $"{productStocksToAdd.Count} records have been added to the database" }
                };
                var addedArray = new JArray();
                foreach (var newProductStock in productStocksToAdd)
                {
                    var productStockObject = new JObject();
                    productStockObject["KitchenProductId"] = newProductStock.Id;
                    productStockObject["KitchenProductName"] = newProductStock.Name;
                    productStockObject["KitchenProductKitchenUnits"] = newProductStock.Units;
                    productStockObject["KitchenProductKitchenUnitType"] = newProductStock.UnitType.ToString();
                    addedArray.Add(productStockObject);
                }
                addedRecordsObject.Add("Results", addedArray);
                inventoryModifiedObject.Add("Added", addedRecordsObject);
            }
            if (productStocksToUpdate.Count > 0)
            {
                var updatedRecordsObject = new JObject
                {
                    { "Updated", $"{productStocksToUpdate.Count} records have been updated in the database" }
                };
                var updatedArray = new JArray();
                foreach (var existingProductStock in productStocksToUpdate)
                {
                    _repository.ProductStocks.Update(existingProductStock);
                    var productStockObject = new JObject();
                    productStockObject["KitchenProductId"] = existingProductStock.Id;
                    productStockObject["KitchenProductName"] = existingProductStock.Name;
                    productStockObject["KitchenProductKitchenUnits"] = existingProductStock.Units;
                    productStockObject["KitchenProductKitchenUnitType"] = existingProductStock.UnitType.ToString();
                    updatedArray.Add(productStockObject);
                }
                updatedRecordsObject.Add("Results", updatedArray);
                inventoryModifiedObject.Add("Updated", updatedRecordsObject);
            }

            model.Response.NavigateToPage = "product-stocks";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(inventoryModifiedObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}