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
    [ChatCommandModel(new [] { "edit_cooked_recipe_ingredient_unittype" })]
    public class ConsumeChatCommandEditCookedRecipeIngredientUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditCookedRecipeIngredientUnitType>
    {
        public ChatAICommandDTOEditCookedRecipeIngredientUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditCookedRecipeIngredientUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditCookedRecipeIngredientUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditCookedRecipeIngredientUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditCookedRecipeIngredientUnitType model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).FirstOrDefault(r => r.Recipe.Name.ToLower().Contains(model.Command.Recipe.ToLower()));

            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find logged recipe by name: " + model.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.Name.ToLower()));
                if (cookedRecipeCalledIngredient == null)
                {
                    var systemResponse = "Could not find ingredient by name: " + model.Command.Name;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    cookedRecipeCalledIngredient.UnitType = model.Command.UnitType.UnitTypeFromString();
                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}