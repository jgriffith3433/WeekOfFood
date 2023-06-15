using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Common;

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
            return "Success";
        }
    }
}