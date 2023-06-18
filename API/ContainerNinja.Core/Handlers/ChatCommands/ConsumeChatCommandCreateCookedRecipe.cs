using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "create_logged_recipe" })]
    public class ConsumeChatCommandCreateCookedRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateCookedRecipe>
    {
        public ChatAICommandDTOCreateCookedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateCookedRecipeHandler : IRequestHandler<ConsumeChatCommandCreateCookedRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateCookedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateCookedRecipe model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);

            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }
            var cookedRecipe = _repository.CookedRecipes.CreateProxy();
            {
                cookedRecipe.Recipe = recipe;
            };
            recipe.CookedRecipes.Add(cookedRecipe);
            foreach (var calledIngredient in recipe.CalledIngredients)
            {
                var cookedRecipeCalledIngredient = _repository.CookedRecipeCalledIngredients.CreateProxy();
                {
                    cookedRecipeCalledIngredient.Name = calledIngredient.Name;
                    cookedRecipeCalledIngredient.CookedRecipe = cookedRecipe;
                    cookedRecipeCalledIngredient.CalledIngredient = calledIngredient;
                    cookedRecipeCalledIngredient.ProductStock = calledIngredient.ProductStock;
                    cookedRecipeCalledIngredient.UnitType = calledIngredient.UnitType;
                    cookedRecipeCalledIngredient.Units = calledIngredient.Units != null ? calledIngredient.Units.Value : 0;
                };
                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
            }
            _repository.Recipes.Update(recipe);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var cookedRecipeObject = new JObject();
            cookedRecipeObject["LoogedRecipeId"] = cookedRecipe.Id;
            if (cookedRecipe.Recipe != null)
            {
                cookedRecipeObject["RecipeName"] = cookedRecipe.Recipe.Name;
                cookedRecipeObject["Serves"] = cookedRecipe.Recipe.Serves;
            }
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in cookedRecipe.CookedRecipeCalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["LoggedIngredientId"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["Units"] = ingredient.Units;
                ingredientObject["UnitType"] = ingredient.UnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            cookedRecipeObject["Ingredients"] = recipeIngredientsArray;
            return "Created log:\n" + JsonConvert.SerializeObject(cookedRecipeObject);
        }
    }
}