using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_for_recipes_with_stocked_products" })]
    public class ConsumeChatCommandSearchForRecipesWithStockedProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchForRecipesWithStockedProducts>
    {
        public ChatAICommandDTOSearchForRecipesWithStockedProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchForRecipesWithStockedProductsHandler : IRequestHandler<ConsumeChatCommandSearchForRecipesWithStockedProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSearchForRecipesWithStockedProductsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchForRecipesWithStockedProducts model, CancellationToken cancellationToken)
        {
            var ids = model.Command.StockedProducts.Select(ps => ps.StockedProductId);
            var recipes = _repository.Recipes.Set.Where(r => r.CalledIngredients.Any(ci => ci.ProductStock != null && ids.Contains(ci.ProductStock.Id)));
            var results = new JArray();
            foreach (var recipe in recipes)
            {
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
                    ingredientObject["IngredientUnits"] = ingredient.Units;
                    ingredientObject["IngredientUnitType"] = ingredient.UnitType.ToString();
                    recipeIngredientsArray.Add(ingredientObject);
                }
                recipeObject["Ingredients"] = recipeIngredientsArray;
                results.Add(recipeObject);
            }
            if (results.Count == 0)
            {
                return $"There are no recipes containing ingredients that are linked to any of the stocked products.";
            }
            model.Response.NavigateToPage = "recipes";
            return JsonConvert.SerializeObject(results);
        }
    }
}