using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_product" })]
    public class ConsumeChatCommandCreateProduct : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOCreateProduct>
    {
        public ChatAICommandDTOCreateProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateProductHandler : IRequestHandler<ConsumeChatCommandCreateProduct, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCreateProduct model, CancellationToken cancellationToken)
        {
            var productEntity = new Product
            {
                Name = model.Command.Product
            };

            //always ensure a product stock record exists for each product
            var productStockEntity = new ProductStock
            {
                Name = model.Command.Product,
                Units = 1
            };
            productStockEntity.Product = productEntity;
            _repository.ProductStocks.Add(productStockEntity);
            model.Response.ChatMessages.Add(new ChatMessageVM
            {
                Content = "Success",
                RawContent = "Success",
                Name = StaticValues.ChatMessageRoles.System,
                Role = StaticValues.ChatMessageRoles.System,
            });
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}