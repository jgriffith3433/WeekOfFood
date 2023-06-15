using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.DTO.ChatAICommands;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "invalid", "unknown" })]
    public class ConsumeChatCommandUnknown : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUnknown>
    {
        public ChatAICommandDTOUnknown Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDefaultHandler : IRequestHandler<ConsumeChatCommandUnknown, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDefaultHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUnknown model, CancellationToken cancellationToken)
        {
            return "Unknown command";
        }
    }
}