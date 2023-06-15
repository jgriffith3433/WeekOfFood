using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "go_to", "go_to_page", "navigate" })]
    [ChatCommandSpecification("go_to_page", "Navigate to page.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""page"": {
            ""type"": ""string"",
            ""enum"": [""home"", ""todo"", ""product stocks"", ""products"", ""completed orders"", ""recipes"", ""logged recipes"", ""called ingredients""],
            ""description"": ""The page to navigate to.""
        }
    },
    ""required"": [""page""]
}")]
    public class ConsumeChatCommandGoToPage : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGoToPage>
    {
        public ChatAICommandDTOGoToPage Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGoToPageHandler : IRequestHandler<ConsumeChatCommandGoToPage, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGoToPageHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGoToPage model, CancellationToken cancellationToken)
        {
            model.Response.NavigateToPage = string.Join('-', model.Command.Page.Split(' '));
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return $"Navigation complete: The user is now on the {model.Command.Page} page.";
        }
    }
}