using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteCookedRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandDeleteCookedRecipeIngredient>
    {
        public ConsumeChatCommandDeleteCookedRecipeIngredientValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName is required");
        }
    }
}
