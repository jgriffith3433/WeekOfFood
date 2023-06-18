using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_logged_recipe_ingredient" })]
    public class ConsumeChatCommandEditCookedRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditCookedRecipeIngredient>
    {
        public ChatAICommandDTOEditCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandEditCookedRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.FirstOrDefault(cr => cr.Id == model.Command.Id);
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find cooked recipe by ID: " + model.Command.Id;
                throw new ChatAIException(systemResponse);
            }
            var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.OriginalIngredient.ToLower()));

            if (cookedRecipeCalledIngredient == null)
            {
                var systemResponse = "Could not find ingredient by name: " + model.Command.OriginalIngredient;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                cookedRecipeCalledIngredient.Name = model.Command.NewIngredient;
                cookedRecipeCalledIngredient.CalledIngredient = null;
                cookedRecipeCalledIngredient.ProductStock = null;
                _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
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
            return $"Substituted ingredient: {model.Command.OriginalIngredient} for {model.Command.NewIngredient}\n" + JsonConvert.SerializeObject(cookedRecipeObject);
        }
    }
}