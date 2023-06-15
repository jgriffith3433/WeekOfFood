using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_recipes" })]
    public class ConsumeChatCommandSearchRecipes : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchRecipes>
    {
        public ChatAICommandDTOSearchRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGoToRecipesHandler : IRequestHandler<ConsumeChatCommandSearchRecipes, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGoToRecipesHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchRecipes model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            var searchTerms = string.Join(' ', model.Command.Search.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.Recipes.Set.AsExpandable().Where(predicate).ToList();
            var results = new JArray();
            foreach (var recipe in query)
            {
                var recipeObject = new JObject();
                recipeObject["RecipeName"] = recipe.Name;
                recipeObject["Serves"] = recipe.Serves;
                var recipeIngredientsArray = new JArray();
                foreach (var ingredient in recipe.CalledIngredients)
                {
                    var ingredientObject = new JObject();
                    ingredientObject["IngredientName"] = ingredient.Name;
                    ingredientObject["Units"] = ingredient.Units;
                    ingredientObject["UnitType"] = ingredient.UnitType.ToString();
                    recipeIngredientsArray.Add(ingredientObject);
                }
                recipeObject["Ingredients"] = recipeIngredientsArray;
                results.Add(recipeObject);
            }
            return JsonConvert.SerializeObject(results);
        }
    }
}