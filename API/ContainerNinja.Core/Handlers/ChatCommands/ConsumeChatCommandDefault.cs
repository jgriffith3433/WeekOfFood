using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.DTO.ChatAICommands;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "invalid", "unknown" })]
    public class ConsumeChatCommandUnknown : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOUnknown>
    {
        public ChatAICommandDTOUnknown Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDefaultHandler : IRequestHandler<ConsumeChatCommandUnknown, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDefaultHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandUnknown request, CancellationToken cancellationToken)
        {
            //default is an unknown command
            var systemResponse = $"Error: unknown cmd '{request.Command.Cmd}'";
            throw new ChatAIException(systemResponse);
        }
    }
}