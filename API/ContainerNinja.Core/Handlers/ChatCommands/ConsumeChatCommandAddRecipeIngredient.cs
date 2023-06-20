using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "add_recipe_ingredient" })]
    public class ConsumeChatCommandAddRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddRecipeIngredient>
    {
        public ChatAICommandDTOAddRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddRecipeIngredient model, CancellationToken cancellationToken)
        {
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
            var recipe = _repository.Recipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var recipeCalledIngredient = _repository.CalledIngredients.CreateProxy();
                {
                    recipeCalledIngredient.Name = model.Command.IngredientName;
                    recipeCalledIngredient.Units = model.Command.Units;
                    recipeCalledIngredient.UnitType = model.Command.UnitType;
                };
                recipe.CalledIngredients.Add(recipeCalledIngredient);
                _repository.Recipes.Update(recipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipe.Id;
            if (recipe != null)
            {
                recipeObject["RecipeName"] = recipe.Name;
                recipeObject["Serves"] = recipe.Serves;
            }
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipe.CalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["IngredientId"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["StockedProductId"] = ingredient.ProductStock?.Id;
                ingredientObject["IngredientUnits"] = ingredient.Units;
                ingredientObject["IngredientUnitType"] = ingredient.UnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            recipeObject["Ingredients"] = recipeIngredientsArray;
            model.Response.NavigateToPage = "recipes";
            return $"Added ingredient: {model.Command.IngredientName}\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}