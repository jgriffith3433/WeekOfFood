using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_recipe_ingredients" })]
    public class ConsumeChatCommandGetRecipeIngredients : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetRecipeIngredients>
    {
        public ChatAICommandDTOGetRecipeIngredients Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetRecipeIngredientsHandler : IRequestHandler<ConsumeChatCommandGetRecipeIngredients, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetRecipeIngredientsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetRecipeIngredients model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            var searchTerms = string.Join(' ', model.Command.RecipeName.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.Recipes.Set.AsExpandable().Where(predicate).ToList();

            Recipe recipe;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.RecipeName.ToLower())
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find recipe by name '" + model.Command.RecipeName + "'. Did you mean: " + query[0].Name + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.RecipeName.ToLower());
                if (exactMatch != null)
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (recipe != null)
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
                return JsonConvert.SerializeObject(recipeObject);
            }
            else
            {
                return $"Could not find recipe by name '{model.Command.RecipeName}'";
            }
        }
    }
}