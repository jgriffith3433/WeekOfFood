using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_recipe_ingredient_unittype" })]
    public class ConsumeChatCommandEditRecipeIngredientUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditRecipeIngredientUnitType>
    {
        public ChatAICommandDTOEditRecipeIngredientUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditRecipeIngredientUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditRecipeIngredientUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditRecipeIngredientUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditRecipeIngredientUnitType model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower().Contains(model.Command.Recipe.ToLower()));

            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.Name.ToLower()));
                if (calledIngredient == null)
                {
                    var systemResponse = "Could not find ingredient by name: " + model.Command.Name;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    calledIngredient.UnitType = model.Command.UnitType.UnitTypeFromString();
                    _repository.CalledIngredients.Update(calledIngredient);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}