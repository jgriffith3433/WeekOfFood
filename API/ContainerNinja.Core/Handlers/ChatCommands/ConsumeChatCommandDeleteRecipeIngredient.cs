using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "remove_recipe_ingredient" })]
    public class ConsumeChatCommandDeleteRecipeIngredient : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTODeleteRecipeIngredient>
    {
        public ChatAICommandDTODeleteRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandDeleteRecipeIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandDeleteRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == model.Command.Recipe.ToLower());
            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + model.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.Ingredient.ToLower()));

                if (calledIngredient == null)
                {
                    var systemResponse = "Error: Could not find ingredient by name: " + model.Command.Ingredient;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    recipe.CalledIngredients.Remove(calledIngredient);
                    _repository.CalledIngredients.Delete(calledIngredient.Id);
                    _repository.Recipes.Update(recipe);
                }
            }
            return model.Response;
        }
    }
}