using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_todo_list" })]
    public class ConsumeChatCommandAddTodoList : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddTodoList>
    {
        public ChatAICommandDTOAddTodoList Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddTodoListHandler : IRequestHandler<ConsumeChatCommandAddTodoList, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddTodoListHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddTodoList model, CancellationToken cancellationToken)
        {
            //Command logic
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return $"Successfully added new to do list {model.Command.ListName}";
        }
    }
}