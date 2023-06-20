using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandAddRecipeIngredient>
    {
        public ConsumeChatCommandAddRecipeIngredientValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId field is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName field is required");
            RuleFor(v => v.Command.Units).NotEmpty().WithMessage("Units field is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType field is required");
        }
    }
}
