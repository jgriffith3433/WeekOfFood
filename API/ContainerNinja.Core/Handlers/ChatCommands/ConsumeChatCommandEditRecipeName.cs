using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_recipe_name" })]
    public class ConsumeChatCommandEditRecipeName : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOEditRecipeName>
    {
        public ChatAICommandDTOEditRecipeName Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditRecipeNameHandler : IRequestHandler<ConsumeChatCommandEditRecipeName, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditRecipeNameHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandEditRecipeName model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.FirstOrDefault(r => r.Name.ToLower().Contains(model.Command.Original.ToLower()));
            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + model.Command.Original;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                recipe.Name = model.Command.New;
                _repository.Recipes.Update(recipe);
            }
            return model.Response;
        }
    }
}