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
    public class ConsumeChatCommandCreateWalmartProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateProduct>
    {
        public ChatAICommandDTOCreateProduct Command { get; set; }
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
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
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