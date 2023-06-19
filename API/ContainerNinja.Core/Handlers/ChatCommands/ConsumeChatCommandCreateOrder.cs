using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Alachisoft.NCache.Common.Protobuf;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_order" })]
    public class ConsumeChatCommandCreateOrder : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateOrder>
    {
        public ChatAICommandDTOCreateOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateOrderHandler : IRequestHandler<ConsumeChatCommandCreateOrder, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateOrder model, CancellationToken cancellationToken)
        {
            foreach(var stockedProductToOrder in model.Command.Products)
            {
                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == stockedProductToOrder.StockedProductId);
                if (productStockEntity == null)
                {
                    var systemResponse = "Could not find product by ID: " + stockedProductToOrder.StockedProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""get_stocked_product_id"" }");
                }

                if (productStockEntity.Product.WalmartId.HasValue == false)
                {
                    var systemResponse = "Stocked Product is not linked to a walmart product yet: " + stockedProductToOrder.StockedProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""link_stocked_product_to_walmart_product"" }");
                }
            }

            var orderEntity = _repository.Orders.CreateProxy();
            _repository.Orders.Add(orderEntity);
            await _repository.CommitAsync();

            foreach (var stockedProductToOrder in model.Command.Products)
            {
                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == stockedProductToOrder.StockedProductId);

                var orderProductEntity = _repository.OrderProducts.CreateProxy();
                {
                    orderProductEntity.Product = productStockEntity.Product;
                    orderProductEntity.WalmartId = productStockEntity.Product.WalmartId;
                    orderProductEntity.Name = productStockEntity.Name;
                }
                _repository.OrderProducts.Add(orderProductEntity);
                orderEntity.OrderProducts.Add(orderProductEntity);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var orderObject = new JObject();
            orderObject["OrderId"] = orderEntity.Id;
            var orderProductsArray = new JArray();
            foreach (var orderProduct in orderEntity.OrderProducts)
            {
                var ingredientObject = new JObject();
                ingredientObject["OrderProductId"] = orderProduct.Id;
                ingredientObject["OrderProductName"] = orderProduct.Name;
                ingredientObject["OrderProductWalmartId"] = orderProduct.WalmartId;
                orderProductsArray.Add(ingredientObject);
            }
            orderObject["OrderProducts"] = orderProductsArray;
            model.Response.NavigateToPage = "orders";
            return "Created order:\n" + JsonConvert.SerializeObject(orderObject);
        }
    }
}