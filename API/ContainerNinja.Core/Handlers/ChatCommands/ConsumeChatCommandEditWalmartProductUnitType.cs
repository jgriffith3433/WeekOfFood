using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_product_unit_type" })]
    public class ConsumeChatCommandEditWalmartProductUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditProductUnitType>
    {
        public ChatAICommandDTOEditProductUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditProductUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditWalmartProductUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditProductUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditWalmartProductUnitType model, CancellationToken cancellationToken)
        {
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
            var walmartProduct = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == model.Command.ProductId);
            if (walmartProduct == null)
            {
                var systemResponse = "Could not find product by ID: " + model.Command.ProductId;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                walmartProduct.UnitType = model.Command.UnitType;
                _repository.WalmartProducts.Update(walmartProduct);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "products";
            return "Success";
        }
    }
}