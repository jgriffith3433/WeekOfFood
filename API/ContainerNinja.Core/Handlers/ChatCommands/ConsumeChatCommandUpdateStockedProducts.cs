//using MediatR;
//using ContainerNinja.Contracts.Data;
//using ContainerNinja.Contracts.DTO.ChatAICommands;
//using ContainerNinja.Contracts.ViewModels;
//using ContainerNinja.Contracts.Data.Entities;
//using ContainerNinja.Core.Common;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
//using ContainerNinja.Contracts.Enum;
//using ContainerNinja.Core.Exceptions;
//using LinqKit;

//namespace ContainerNinja.Core.Handlers.ChatCommands
//{
//    [ChatCommandModel(new[] { "update_kitchen_products" })]
//    public class ConsumeChatCommandUpdateStockedProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateStockedProducts>
//    {
//        public ChatAICommandDTOUpdateStockedProducts Command { get; set; }
//        public ChatResponseVM Response { get; set; }
//    }

//    public class ConsumeChatCommandUpdateProductStockHandler : IRequestHandler<ConsumeChatCommandUpdateStockedProducts, string>
//    {
//        private readonly IUnitOfWork _repository;

//        public ConsumeChatCommandUpdateProductStockHandler(IUnitOfWork repository)
//        {
//            _repository = repository;
//        }

//        public async Task<string> Handle(ConsumeChatCommandUpdateStockedProducts model, CancellationToken cancellationToken)
//        {
//            var productStocksToUpdate = new List<ProductStock>();
//            //first go through all the Ids we have and check if they all exist
//            foreach (var item in model.Command.StockedProducts)
//            {
//                if (item.StockedProductId.HasValue)
//                {
//                    var existingProductStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == item.StockedProductId);
//                    if (existingProductStockEntity == null)
//                    {
//                        throw new ChatAIException($"Could not find stocked product by ID: {item.StockedProductId}", @"{ ""name"": ""search_stocked_products"" }");
//                    }
//                    existingProductStockEntity.Units = item.Units;
//                    existingProductStockEntity.UnitType = item.KitchenUnitType;
//                    productStocksToUpdate.Add(existingProductStockEntity);
//                }
//            }
//            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
//            await _repository.CommitAsync();

//            foreach (var existingProductStock in productStocksToUpdate)
//            {
//                _repository.ProductStocks.Update(existingProductStock);
//            }

//            var updatedObject = new JObject
//            {
//                { "Flow", "User is updating their kitchen inventory" }
//            };
//            if (productStocksToUpdate.Count > 0)
//            {
//                var updatedRecordsObject = new JObject
//                {
//                    { "Message", $"{productStocksToUpdate.Count} records have been updated in the database" }
//                };
//                var updatedArray = new JArray();
//                foreach (var existingProductStock in productStocksToUpdate)
//                {
//                    var productStockObject = new JObject();
//                    productStockObject["KitchenProductId"] = existingProductStock.Id;
//                    productStockObject["KitchenProductName"] = existingProductStock.Name;
//                    productStockObject["KitchenProductKitchenUnits"] = existingProductStock.Units;
//                    productStockObject["KitchenProductKitchenUnitType"] = existingProductStock.UnitType.ToString();
//                    updatedArray.Add(productStockObject);
//                }
//                updatedRecordsObject.Add("Results", updatedArray);
//                updatedObject.Add("Updated", updatedRecordsObject);
//            }

//            model.Response.NavigateToPage = "product-stocks";
//            model.Response.ForceFunctionCall = "none";
//            return JsonConvert.SerializeObject(updatedObject, new JsonSerializerSettings()
//            {
//                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//            });
//        }
//    }
//}