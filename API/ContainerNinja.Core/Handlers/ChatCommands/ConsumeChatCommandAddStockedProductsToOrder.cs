using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_items_to_order" })]
    public class ConsumeChatCommandAddStockedProductsToOrder : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddStockedProductsToOrder>
    {
        public ChatAICommandDTOAddStockedProductsToOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddStockedProductsToOrderHandler : IRequestHandler<ConsumeChatCommandAddStockedProductsToOrder, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddStockedProductsToOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddStockedProductsToOrder model, CancellationToken cancellationToken)
        {
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
            var orderEntity = _repository.Orders.Set.FirstOrDefault(o => o.Id == model.Command.OrderId);
            if (orderEntity == null)
            {
                var systemResponse = "Could not find order by ID: " + model.Command.OrderId;
                throw new ChatAIException(systemResponse);//, @"{ ""name"": ""get_order_id"" }");
            }

            var notLinkedToWalmartProducts = new List<int>();
            var noUnitConversionToWalmartProducts = new List<int>();
            foreach (var stockedProductToOrder in model.Command.StockedProducts)
            {
                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == stockedProductToOrder.StockedProductId);
                if (productStockEntity == null)
                {
                    var systemResponse = "Could not find stocked product by ID: " + stockedProductToOrder.StockedProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""get_stocked_product_id"" }");
                }

                if (productStockEntity.WalmartProduct == null)
                {
                    notLinkedToWalmartProducts.Add(stockedProductToOrder.StockedProductId);
                    var systemResponse = $"Stocked Product ({stockedProductToOrder.StockedProductId}) is not linked to a walmart product yet";
                }

                if (productStockEntity.WalmartProduct.WalmartId.HasValue == false)
                {
                    notLinkedToWalmartProducts.Add(stockedProductToOrder.StockedProductId);
                    var systemResponse = $"Stocked Product ({stockedProductToOrder.StockedProductId}) is not linked to a walmart product yet";
                }
            }
            if (notLinkedToWalmartProducts.Count > 0)
            {
                var systemResponse = "The following stocked products are not linked to walmart products: " + string.Join(", ", notLinkedToWalmartProducts);
                throw new ChatAIException(systemResponse, @"{ ""name"": ""link_stocked_products_to_walmart_products"" }");
            }

            foreach (var stockedProductToOrder in model.Command.StockedProducts)
            {
                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == stockedProductToOrder.StockedProductId);

                var orderItemEntity = _repository.OrderItems.CreateProxy();
                {
                    orderItemEntity.Product = productStockEntity.WalmartProduct;
                    orderItemEntity.WalmartId = productStockEntity.WalmartProduct.WalmartId;
                    orderItemEntity.Name = productStockEntity.Name;
                    orderItemEntity.Quantity = stockedProductToOrder.Quantity;
                }
                _repository.OrderItems.Add(orderItemEntity);
                orderEntity.OrderItems.Add(orderItemEntity);
            }
            _repository.Orders.Update(orderEntity);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var orderObject = new JObject();
            orderObject["OrderId"] = orderEntity.Id;
            var orderProductsArray = new JArray();
            foreach (var orderProduct in orderEntity.OrderItems)
            {
                var ingredientObject = new JObject();
                ingredientObject["OrderItemId"] = orderProduct.Id;
                ingredientObject["OrderItemName"] = orderProduct.Name;
                ingredientObject["OrderItemWalmartId"] = orderProduct.WalmartId;
                orderProductsArray.Add(ingredientObject);
            }
            orderObject["OrderItems"] = orderProductsArray;
            model.Response.NavigateToPage = "orders";
            return JsonConvert.SerializeObject(orderObject);
        }
    }
}