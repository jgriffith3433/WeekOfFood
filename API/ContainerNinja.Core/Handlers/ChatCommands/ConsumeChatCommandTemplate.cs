using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "Template" })]
    public class ConsumeChatCommandTemplate : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOTemplate>
    {
        public ChatAICommandDTOTemplate Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandTemplateHandler : IRequestHandler<ConsumeChatCommandTemplate, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandTemplateHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandTemplate model, CancellationToken cancellationToken)
        {
            //Command logic

            return model.Response;
        }
    }
}