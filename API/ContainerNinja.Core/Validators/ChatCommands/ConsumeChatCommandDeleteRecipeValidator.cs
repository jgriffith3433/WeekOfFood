using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteRecipeValidator : AbstractValidator<ConsumeChatCommandDeleteRecipe>
    {
        public ConsumeChatCommandDeleteRecipeValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
        }
    }
}
