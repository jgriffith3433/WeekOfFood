using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_recipe_ingredient" })]
    public class ConsumeChatCommandAddRecipeIngredient : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOAddRecipeIngredient>
    {
        public ChatAICommandDTOAddRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddRecipeIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandAddRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == model.Command.Recipe.ToLower());
            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + model.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var calledIngredient = new CalledIngredient
                {
                    Name = model.Command.Name,
                    Recipe = recipe,
                    Verified = false,
                    Units = model.Command.Units,
                    UnitType = model.Command.UnitType.UnitTypeFromString()
                };
                recipe.CalledIngredients.Add(calledIngredient);
                _repository.Recipes.Update(recipe);
            }
            return model.Response;
        }
    }
}