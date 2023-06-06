using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "none" })]
    public class ConsumeChatCommandNone : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTONone>
    {
        public ChatAICommandDTONone Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandNoneHandler : IRequestHandler<ConsumeChatCommandNone, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandNoneHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandNone model, CancellationToken cancellationToken)
        {
            return model.Response;
        }
    }
}