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
    public class ConsumeChatCommandEditProductUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditProductUnitType>
    {
        public ChatAICommandDTOEditProductUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditProductUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditProductUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditProductUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditProductUnitType model, CancellationToken cancellationToken)
        {
            var product = _repository.Products.Set.FirstOrDefault(p => p.Id == model.Command.ProductId);
            if (product == null)
            {
                var systemResponse = "Could not find product by ID: " + model.Command.ProductId;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                product.UnitType = model.Command.UnitType.UnitTypeFromString();
                _repository.Products.Update(product);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}