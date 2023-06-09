using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_recipe" })]
    public class ConsumeChatCommandCreateRecipe : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOCreateRecipe>
    {
        public ChatAICommandDTOCreateRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateRecipeHandler : IRequestHandler<ConsumeChatCommandCreateRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCreateRecipe model, CancellationToken cancellationToken)
        {
            var recipeEntity = new Recipe();

            recipeEntity.Name = model.Command.Name;

            foreach (var createRecipeIngredient in model.Command.Ingredients)
            {
                var calledIngredient = new CalledIngredient
                {
                    Name = createRecipeIngredient.Name,
                    Recipe = recipeEntity,
                    Verified = false,
                    Units = createRecipeIngredient.Units,
                    UnitType = createRecipeIngredient.UnitType.UnitTypeFromString()
                };
                recipeEntity.CalledIngredients.Add(calledIngredient);
            }

            _repository.Recipes.Add(recipeEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}