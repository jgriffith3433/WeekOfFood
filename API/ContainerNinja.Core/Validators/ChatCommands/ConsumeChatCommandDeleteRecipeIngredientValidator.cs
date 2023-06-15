using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandDeleteRecipeIngredient>
    {
        public ConsumeChatCommandDeleteRecipeIngredientValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName is required");
        }
    }
}
