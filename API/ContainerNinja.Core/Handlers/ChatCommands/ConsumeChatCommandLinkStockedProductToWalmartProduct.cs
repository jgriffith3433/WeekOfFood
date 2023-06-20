using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "link_stocked_products_to_walmart_products" })]
    public class ConsumeChatCommandLinkStockedProductToWalmartProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOLinkStockedProductsToWalmartProducts>
    {
        public ChatAICommandDTOLinkStockedProductsToWalmartProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandLinkStockedProductToWalmartProductHandler : IRequestHandler<ConsumeChatCommandLinkStockedProductToWalmartProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandLinkStockedProductToWalmartProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandLinkStockedProductToWalmartProduct model, CancellationToken cancellationToken)
        {
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
            var notFoundWalmartIds = new List<long>();
            var stockedProductsToUpdate = new List<ProductStock>();
            foreach (var link in model.Command.Links)
            {
                var stockedProductEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Id == link.StockedProductId);
                if (stockedProductEntity == null)
                {
                    var systemResponse = "Could not find stocked product by ID: " + link.StockedProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""get_stocked_product_id"" }");
                }

                var walmartItemResult = await _walmartService.GetItem(link.WalmartProductId);
                if (walmartItemResult == null)
                {
                    notFoundWalmartIds.Add(link.WalmartProductId);
                    continue;
                }
                stockedProductEntity.Product.WalmartId = link.WalmartProductId;
                stockedProductsToUpdate.Add(stockedProductEntity);
            }
            if (notFoundWalmartIds.Count > 0)
            {
                //var systemResponse = "Could not find the following walmart products by ID: " + string.Join(", ", notFoundWalmartIds);
                //throw new ChatAIException(systemResponse, @"{ ""name"": ""search_walmart_products_for_stocked_product"" }");
            }

            foreach (var stockedProductToUpdate in stockedProductsToUpdate)
            {
                _repository.ProductStocks.Update(stockedProductToUpdate);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "products";
            return $"Successfully linked {model.Command.Links.Count} walmart products";
        }
    }
}