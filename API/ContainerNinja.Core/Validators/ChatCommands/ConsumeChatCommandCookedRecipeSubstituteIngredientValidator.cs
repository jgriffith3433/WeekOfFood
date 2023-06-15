using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCookedRecipeSubstituteIngredientValidator : AbstractValidator<ConsumeChatCommandSubstituteCookedRecipeIngredient>
    {
        public ConsumeChatCommandCookedRecipeSubstituteIngredientValidator()
        {
            RuleFor(v => v.Command.OriginalIngredient).NotEmpty().WithMessage("OriginalIngredient is required");
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
            RuleFor(v => v.Command.NewIngredient).NotEmpty().WithMessage("NewIngredient is required");
        }
    }
}
