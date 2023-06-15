using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_to_do_list" })]
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
            return "Success";
        }
    }
}