using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditCookedRecipeIngredientUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeIngredientUnitType>
    {
        public ConsumeChatCommandEditCookedRecipeIngredientUnitTypeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId field is required");
            RuleFor(v => v.Command.LoggedIngredientId).NotEmpty().WithMessage("LoggedIngredientId field is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType field is required");
        }
    }
}
