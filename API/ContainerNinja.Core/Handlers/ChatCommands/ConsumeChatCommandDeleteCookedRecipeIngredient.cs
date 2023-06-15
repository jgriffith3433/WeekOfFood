using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "remove_logged_recipe_ingredient" })]
    [ChatCommandSpecification("remove_logged_recipe_ingredient", "Delete an ingredient from a logged recipe.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""recipename"": {
            ""type"": ""string"",
            ""description"": ""The name of the logged recipe.""
        },
        ""ingredientname"": {
            ""type"": ""string"",
            ""description"": ""The name of the ingredient to delete.""
        }
    },
    ""required"": [""recipename"", ""ingredientname""]
}")]
    public class ConsumeChatCommandDeleteCookedRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteCookedRecipeIngredient>
    {
        public ChatAICommandDTODeleteCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandDeleteCookedRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).FirstOrDefault(r => r.Recipe.Name.ToLower() == model.Command.RecipeName.ToLower());
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find logged recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.IngredientName.ToLower()));

                if (cookedRecipeCalledIngredient == null)
                {
                    var systemResponse = "Could not find ingredient by name: " + model.Command.IngredientName;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    cookedRecipe.CookedRecipeCalledIngredients.Remove(cookedRecipeCalledIngredient);
                    _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredient.Id);
                    _repository.CookedRecipes.Update(cookedRecipe);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return $"Removed {model.Command.IngredientName} from logged recipe {model.Command.RecipeName}";
        }
    }
}
