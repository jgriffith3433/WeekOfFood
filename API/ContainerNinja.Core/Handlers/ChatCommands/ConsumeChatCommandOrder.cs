using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "order" })]
    public class ConsumeChatCommandOrder : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOOrder>
    {
        public ChatAICommandDTOOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandOrderHandler : IRequestHandler<ConsumeChatCommandOrder, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandOrder model, CancellationToken cancellationToken)
        {
            var systemResponse = "Not implemented.";
            throw new ChatAIException(systemResponse);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}