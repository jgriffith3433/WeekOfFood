using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_product_unit_type" })]
    public class ConsumeChatCommandEditProductUnitType : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOEditProductUnitType>
    {
        public ChatAICommandDTOEditProductUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditProductUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditProductUnitType, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditProductUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandEditProductUnitType model, CancellationToken cancellationToken)
        {
            var product = _repository.Products.FirstOrDefault(p => p.Name.ToLower().Contains(model.Command.Product.ToLower()));
            if (product == null)
            {
                var systemResponse = "Error: Could not find product by name: " + model.Command.Product;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                product.UnitType = model.Command.UnitType.UnitTypeFromString();
                _repository.Products.Update(product);
            }
            return model.Response;
        }
    }
}