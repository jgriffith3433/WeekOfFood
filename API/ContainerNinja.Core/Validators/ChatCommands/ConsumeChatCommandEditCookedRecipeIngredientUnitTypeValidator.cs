using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditCookedRecipeIngredientUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeIngredientUnitType>
    {
        public ConsumeChatCommandEditCookedRecipeIngredientUnitTypeValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId is required");
            RuleFor(v => v.Command.LoggedIngredientId).NotEmpty().WithMessage("LoggedIngredientId is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType is required");
        }
    }
}
