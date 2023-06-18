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
    [ChatCommandModel(new [] { "edit_recipe_ingredient" })]
    public class ConsumeChatCommandEditRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditRecipeIngredient>
    {
        public ChatAICommandDTOEditRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandEditRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.Id);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.Id;
                throw new ChatAIException(systemResponse);
            }
            var calledIngredient = recipeEntity.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.OriginalIngredient.ToLower()));

            if (calledIngredient == null)
            {
                var systemResponse = "Could not find ingredient by name: " + model.Command.OriginalIngredient;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                calledIngredient.Name = model.Command.NewIngredient;
                calledIngredient.ProductStock = null;
                _repository.CalledIngredients.Update(calledIngredient);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            var recipeObject = new JObject();
            recipeObject["Id"] = recipeEntity.Id;
            recipeObject["RecipeName"] = recipeEntity.Name;
            recipeObject["Serves"] = recipeEntity.Serves;
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipeEntity.CalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["Id"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["Units"] = ingredient.Units;
                ingredientObject["UnitType"] = ingredient.UnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            recipeObject["Ingredients"] = recipeIngredientsArray;
            return "Recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}