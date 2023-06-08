using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "go_to", "go_to_page", "navigate" })]
    public class ConsumeChatCommandGoToPage : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOGoToPage>
    {
        public ChatAICommandDTOGoToPage Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGoToPageHandler : IRequestHandler<ConsumeChatCommandGoToPage, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGoToPageHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandGoToPage model, CancellationToken cancellationToken)
        {
            model.Response.NavigateToPage = string.Join('-', model.Command.Page.Split(' '));
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