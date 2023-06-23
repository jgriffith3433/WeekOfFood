using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_walmart_product" })]
    public class ConsumeChatCommandCreateWalmartProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateWalmartProduct>
    {
        public ChatAICommandDTOCreateWalmartProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateWalmartProductHandler : IRequestHandler<ConsumeChatCommandCreateWalmartProduct, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateWalmartProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateWalmartProduct model, CancellationToken cancellationToken)
        {
            var existingProductWithName = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Name.ToLower() == model.Command.ProductName.ToLower());

            if (existingProductWithName != null)
            {
                var systemResponse = "Product already exists: " + model.Command.ProductName;
                throw new ChatAIException(systemResponse);
            }

            var productEntity = _repository.WalmartProducts.CreateProxy();
            {
                productEntity.Name = model.Command.ProductName;
            };
            _repository.WalmartProducts.Add(productEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "products";
            return $"Successfully created product {model.Command.ProductName}";
        }
    }
}