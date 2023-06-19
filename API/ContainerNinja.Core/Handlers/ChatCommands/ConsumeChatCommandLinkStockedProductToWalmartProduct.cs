using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "link_stocked_product_to_walmart_product" })]
    public class ConsumeChatCommandLinkStockedProductToWalmartProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOLinkStockedProductToWalmartProduct>
    {
        public ChatAICommandDTOLinkStockedProductToWalmartProduct Command { get; set; }
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
            var stockedProductEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Id == model.Command.StockedProductId);
            if (stockedProductEntity == null)
            {
                var systemResponse = "Could not find stocked product by ID: " + model.Command.StockedProductId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_stocked_product_id"" }");
            }
            var walmartItemResult = await _walmartService.GetItem(model.Command.WalmartProductId);
            if (walmartItemResult == null)
            {
                var systemResponse = "Could not find walmart product by ID: " + model.Command.WalmartProductId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""search_walmart_products"" }");
            }
            stockedProductEntity.Product.WalmartId = model.Command.WalmartProductId;
            _repository.ProductStocks.Update(stockedProductEntity);
            //see UpdateProductCommand.cs

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "products";
            return $"Successfully linked StockedProductId: {model.Command.StockedProductId} to WalmartProductId: {model.Command.WalmartProductId}";
        }
    }
}