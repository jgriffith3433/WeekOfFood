using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_recipe_ingredient_unit_type" })]
    public class ConsumeChatCommandEditRecipeIngredientUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditRecipeIngredientUnitType>
    {
        public ChatAICommandDTOEditRecipeIngredientUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditRecipeIngredientUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditRecipeIngredientUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditRecipeIngredientUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditRecipeIngredientUnitType model, CancellationToken cancellationToken)
        {
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
            var recipe = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);

            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }
            var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Id == model.Command.IngredientId);
            if (calledIngredient == null)
            {
                var systemResponse = "Could not find ingredient by ID: " + model.Command.IngredientId;
                throw new ChatAIException(systemResponse);
            }
            calledIngredient.UnitType = model.Command.UnitType.UnitTypeFromString();
            _repository.CalledIngredients.Update(calledIngredient);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipe.Id;
            recipeObject["RecipeName"] = recipe.Name;
            recipeObject["Serves"] = recipe.Serves;
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipe.CalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["IngredientId"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["Units"] = ingredient.Units;
                ingredientObject["UnitType"] = ingredient.UnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            recipeObject["Ingredients"] = recipeIngredientsArray;
            model.Response.NavigateToPage = "recipes";
            return "Recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}