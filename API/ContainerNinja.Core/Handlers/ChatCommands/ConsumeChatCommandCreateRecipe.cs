using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "create_recipe" })]
    public class ConsumeChatCommandCreateRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateRecipe>
    {
        public ChatAICommandDTOCreateRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateRecipeHandler : IRequestHandler<ConsumeChatCommandCreateRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateRecipe model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.CreateProxy();
            recipeEntity.Name = model.Command.RecipeName;
            _repository.Recipes.Add(recipeEntity);

            foreach (var createRecipeIngredient in model.Command.Ingredients)
            {
                var calledIngredient = _repository.CalledIngredients.CreateProxy();
                {
                    calledIngredient.Name = createRecipeIngredient.IngredientName;
                    calledIngredient.Recipe = recipeEntity;
                    calledIngredient.Verified = false;
                    calledIngredient.Units = createRecipeIngredient.Units;
                    calledIngredient.UnitType = createRecipeIngredient.UnitType.UnitTypeFromString();
                };

                recipeEntity.CalledIngredients.Add(calledIngredient);

                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Name.ToLower() == createRecipeIngredient.IngredientName.ToLower());

                if (productStockEntity == null)
                {
                    var productEntity = _repository.Products.CreateProxy();
                    {
                        productEntity.Name = createRecipeIngredient.IngredientName;
                        productEntity.UnitType = createRecipeIngredient.UnitType.UnitTypeFromString();
                    };

                    //always ensure a product stock record exists for each product
                    productStockEntity = _repository.ProductStocks.CreateProxy();
                    {
                        productStockEntity.Name = createRecipeIngredient.IngredientName;
                        productStockEntity.Units = 1;
                    };
                    productStockEntity.Product = productEntity;
                    _repository.ProductStocks.Add(productStockEntity);
                }

                calledIngredient.ProductStock = productStockEntity;
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipeEntity.CalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["IngredientId"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["StockedProductId"] = ingredient.ProductStock?.Id;
                recipeIngredientsArray.Add(ingredientObject);
            }
            recipeObject["Ingredients"] = recipeIngredientsArray;
            model.Response.NavigateToPage = "recipes";
            return "Created recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}