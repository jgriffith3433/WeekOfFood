using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeIngredientUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditRecipeIngredientUnitType>
    {
        public ConsumeChatCommandEditRecipeIngredientUnitTypeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId field is required");
            RuleFor(v => v.Command.IngredientId).NotEmpty().WithMessage("IngredientId field is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType field is required");
        }
    }
}
