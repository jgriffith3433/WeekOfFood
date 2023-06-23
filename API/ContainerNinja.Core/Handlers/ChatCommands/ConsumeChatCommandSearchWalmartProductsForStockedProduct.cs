using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json.Linq;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_walmart_products_for_stocked_product" })]
    public class ConsumeChatCommandSearchWalmartProductsForStockedProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchWalmartProductsForStockedProduct>
    {
        public ChatAICommandDTOSearchWalmartProductsForStockedProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchWalmartProductsForStockedProductHandler : IRequestHandler<ConsumeChatCommandSearchWalmartProductsForStockedProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandSearchWalmartProductsForStockedProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchWalmartProductsForStockedProduct model, CancellationToken cancellationToken)
        {
            var stockedProductsToFindWalmartProducts = new List<ProductStock>();
            var productStockWalmartProducts = new JArray();
            foreach(var id in model.Command.StockedProductIds)
            {
                var stockedProductEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Id == id);
                if (stockedProductEntity == null)
                {
                    var systemResponse = "Could not find stocked product by ID: " + id;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_stocked_products"" }");
                }
                var walmartProductsArray = new JArray();
                var walmartSearchResults = await _walmartService.Search(stockedProductEntity.Name);
                if (walmartSearchResults != null && walmartSearchResults.items != null)
                {
                    foreach (var walmartItem in walmartSearchResults.items)
                    {
                        var walmartProductObject = new JObject();
                        walmartProductObject["WalmartId"] = walmartItem.itemId;
                        walmartProductObject["WalmartProductName"] = walmartItem.name;
                        walmartProductObject["WalmartProductSize"] = walmartItem.size;
                        walmartProductsArray.Add(walmartProductObject);
                    }
                }
                var productStockWalmartProductsObject = new JObject();
                productStockWalmartProductsObject["StockedProductId"] = stockedProductEntity.Id;
                productStockWalmartProductsObject["WalmartSearchResults"] = walmartProductsArray;
                productStockWalmartProducts.Add(productStockWalmartProductsObject);
            }
            model.Response.ForceFunctionCall = "none";
            model.Response.NavigateToPage = "products";
            return JsonConvert.SerializeObject(productStockWalmartProducts, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}