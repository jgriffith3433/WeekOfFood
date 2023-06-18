using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeIngredientUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditRecipeIngredientUnitType>
    {
        public ConsumeChatCommandEditRecipeIngredientUnitTypeValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId is required");
            RuleFor(v => v.Command.IngredientId).NotEmpty().WithMessage("IngredientId is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType is required");
        }
    }
}
