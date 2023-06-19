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
    [ChatCommandModel(new[] { "get_all_recipes"})]
    public class ConsumeChatCommandGetAllRecipes : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetAllRecipes>
    {
        public ChatAICommandDTOGetAllRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetAllRecipesHandler : IRequestHandler<ConsumeChatCommandGetAllRecipes, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetAllRecipesHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetAllRecipes model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            if (string.IsNullOrEmpty(model.Command.Search))
            {
                predicate = predicate.Or(r => true);
            }
            else
            {
                var searchTerms = string.Join(' ', model.Command.Search.ToLower().Split('-')).Split(' ');
                foreach (var searchTerm in searchTerms)
                {
                    predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm));
                }
            }
            var query = _repository.Recipes.Set.AsExpandable().Where(predicate).ToList();
            var results = new JArray();
            foreach (var recipe in query)
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
                    ingredientObject["StockedProductId"] = ingredient.ProductStock?.Id;
                    ingredientObject["Units"] = ingredient.Units;
                    ingredientObject["UnitType"] = ingredient.UnitType.ToString();
                    recipeIngredientsArray.Add(ingredientObject);
                }
                recipeObject["Ingredients"] = recipeIngredientsArray;
                results.Add(recipeObject);
            }
            if (results.Count == 0)
            {
                return $"No Recipes matching the search term: {model.Command.Search}";
            }
            model.Response.NavigateToPage = "recipes";
            return "Recipes:\n" + JsonConvert.SerializeObject(results);
        }
    }
}