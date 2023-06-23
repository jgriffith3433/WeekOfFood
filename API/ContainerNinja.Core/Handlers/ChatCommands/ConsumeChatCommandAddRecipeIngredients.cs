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
    [ChatCommandModel(new[] { "update_recipe" })]
    public class ConsumeChatCommandUpdateRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateRecipe>
    {
        public ChatAICommandDTOUpdateRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddRecipeIngredientsHandler : IRequestHandler<ConsumeChatCommandUpdateRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddRecipeIngredientsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUpdateRecipe model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }
            recipeEntity.Name = model.Command.RecipeName;
            recipeEntity.Serves = model.Command.Serves;

            foreach (var addRecipeIngredient in model.Command.Ingredients)
            {
                var calledIngredientEntity = _repository.CalledIngredients.CreateProxy();
                {
                    calledIngredientEntity.Name = addRecipeIngredient.IngredientName;
                    calledIngredientEntity.Units = addRecipeIngredient.Units;
                    calledIngredientEntity.UnitType = addRecipeIngredient.KitchenUnitType;
                };
                recipeEntity.CalledIngredients.Add(calledIngredientEntity);
                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Name.ToLower() == addRecipeIngredient.IngredientName.ToLower());

                if (productStockEntity == null)
                {
                    productStockEntity = _repository.ProductStocks.CreateProxy();
                    {
                        productStockEntity.Name = addRecipeIngredient.IngredientName;
                    };
                    _repository.ProductStocks.Add(productStockEntity);
                }

                calledIngredientEntity.ProductStock = productStockEntity;
            }
            _repository.Recipes.Update(recipeEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            if (recipeEntity != null)
            {
                recipeObject["RecipeName"] = recipeEntity.Name;
                recipeObject["Serves"] = recipeEntity.Serves;
            }
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipeEntity.CalledIngredients)
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
            return JsonConvert.SerializeObject(recipeObject);
        }
    }
}