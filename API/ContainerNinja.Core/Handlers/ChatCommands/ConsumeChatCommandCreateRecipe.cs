using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_recipe" })]
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
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

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
            return "Created recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}