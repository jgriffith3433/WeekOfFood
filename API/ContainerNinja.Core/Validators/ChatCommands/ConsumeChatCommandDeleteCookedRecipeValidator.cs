using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteCookedRecipeValidator : AbstractValidator<ConsumeChatCommandDeleteCookedRecipe>
    {
        public ConsumeChatCommandDeleteCookedRecipeValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
        }
    }
}
