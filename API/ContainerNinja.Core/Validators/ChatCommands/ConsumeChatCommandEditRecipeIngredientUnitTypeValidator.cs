using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeIngredientUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditRecipeIngredientUnitType>
    {
        public ConsumeChatCommandEditRecipeIngredientUnitTypeValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType is required");
        }
    }
}
