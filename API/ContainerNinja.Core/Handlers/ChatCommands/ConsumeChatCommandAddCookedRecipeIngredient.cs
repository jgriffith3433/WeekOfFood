using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "add_logged_recipe_ingredient" })]
    public class ConsumeChatCommandAddCookedRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddCookedRecipeIngredient>
    {
        public ChatAICommandDTOAddCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddCookedRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Name.ToLower() == model.Command.RecipeName.ToLower());
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find logged recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = _repository.CookedRecipeCalledIngredients.CreateProxy();
                {
                    cookedRecipeCalledIngredient.Name = model.Command.IngredientName;
                    //cookedRecipeCalledIngredient.CookedRecipe = cookedRecipe;
                    cookedRecipeCalledIngredient.Units = model.Command.Units;
                    cookedRecipeCalledIngredient.UnitType = model.Command.UnitType.UnitTypeFromString();
                };
                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                _repository.CookedRecipes.Update(cookedRecipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            var cookedRecipeObject = new JObject();
            cookedRecipeObject["Id"] = cookedRecipe.Id;
            if (cookedRecipe.Recipe != null)
            {
                cookedRecipeObject["RecipeName"] = cookedRecipe.Recipe.Name;
                cookedRecipeObject["Serves"] = cookedRecipe.Recipe.Serves;
            }
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in cookedRecipe.CookedRecipeCalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["Id"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["Units"] = ingredient.Units;
                ingredientObject["UnitType"] = ingredient.UnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            cookedRecipeObject["Ingredients"] = recipeIngredientsArray;
            return $"Added ingredient: {model.Command.IngredientName}\n" + JsonConvert.SerializeObject(cookedRecipeObject);
        }
    }
}