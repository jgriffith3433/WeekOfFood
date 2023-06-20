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
            if (model.Command.UserGavePermission == null || model.Command.UserGavePermission == false)
            {
                model.Response.ForceFunctionCall = "none";
                return "Ask for permission";
            }
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
                    calledIngredient.UnitType = createRecipeIngredient.UnitType;
                };

                recipeEntity.CalledIngredients.Add(calledIngredient);

                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Name.ToLower() == createRecipeIngredient.IngredientName.ToLower());

                if (productStockEntity == null)
                {
                    productStockEntity = _repository.ProductStocks.CreateProxy();
                    {
                        productStockEntity.Name = createRecipeIngredient.IngredientName;
                    };
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
                recipeIngredientsArray.Add(ingredientObject);
            }
            recipeObject["Ingredients"] = recipeIngredientsArray;
            model.Response.NavigateToPage = "recipes";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(recipeObject);
        }
    }
}