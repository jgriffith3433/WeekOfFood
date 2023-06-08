using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "order" })]
    public class ConsumeChatCommandOrder : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOOrder>
    {
        public ChatAICommandDTOOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandOrderHandler : IRequestHandler<ConsumeChatCommandOrder, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandOrder model, CancellationToken cancellationToken)
        {
            var systemResponse = "Error: Not implemented: " + model.Command.Cmd;
            throw new ChatAIException(systemResponse);
            model.Response.ChatMessages.Add(new ChatMessageVM
            {
                Content = "Success",
                RawContent = "Success",
                Name = StaticValues.ChatMessageRoles.System,
                Role = StaticValues.ChatMessageRoles.System,
            });
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}