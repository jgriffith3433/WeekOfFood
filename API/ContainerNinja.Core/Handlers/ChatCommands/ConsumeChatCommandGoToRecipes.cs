using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "go_to_recipes" })]
    public class ConsumeChatCommandGoToRecipes : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOGoToRecipes>
    {
        public ChatAICommandDTOGoToRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGoToRecipesHandler : IRequestHandler<ConsumeChatCommandGoToRecipes, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGoToRecipesHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandGoToRecipes model, CancellationToken cancellationToken)
        {
            model.Response.NavigateToPage = "recipes";
            return model.Response;
        }
    }
}