using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandRecipeSubstituteIngredientValidator : AbstractValidator<ConsumeChatCommandSubstituteRecipeIngredient>
    {
        public ConsumeChatCommandRecipeSubstituteIngredientValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName required");
            RuleFor(v => v.Command.OriginalIngredient).NotEmpty().WithMessage("OriginalIngredient required");
            RuleFor(v => v.Command.NewIngredient).NotEmpty().WithMessage("NewIngredient required");
        }
    }
}
