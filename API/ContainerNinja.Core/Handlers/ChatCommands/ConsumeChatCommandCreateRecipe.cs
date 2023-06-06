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
            //var response = _mediator.Send(new CreateRecipeCommand
            //{
            //    Name = createRecipe.Name,
            //    UserImport = notification.ChatCommand.RawReponse
            //});
            var recipeEntity = new Recipe();

            recipeEntity.Name = model.Command.Name;
            //entity.Serves = createRecipe.Serves.Value;
            //recipeEntity.UserImport = model.RawChatAICommand;

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
            //await _repository.CommitAsync();

            //if (entity.UserImport != null)
            //{
            //    entity.AddDomainEvent(new RecipeUserImportEvent(entity));
            //}
            //await _repository.SaveChangesAsync(cancellationToken);
            return model.Response;
        }
    }
}