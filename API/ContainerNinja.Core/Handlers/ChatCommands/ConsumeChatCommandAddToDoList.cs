using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_to_do_list" })]
    public class ConsumeChatCommandAddTodoList : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOAddTodoList>
    {
        public ChatAICommandDTOAddTodoList Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddTodoListHandler : IRequestHandler<ConsumeChatCommandAddTodoList, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddTodoListHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandAddTodoList model, CancellationToken cancellationToken)
        {
            //Command logic

            return model.Response;
        }
    }
}