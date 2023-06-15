using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandGetRecipeIngredientsValidator : AbstractValidator<ConsumeChatCommandGetRecipeIngredients>
    {
        public ConsumeChatCommandGetRecipeIngredientsValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
        }
    }
}
