using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateCookedRecipeValidator : AbstractValidator<ConsumeChatCommandCreateCookedRecipe>
    {
        public ConsumeChatCommandCreateCookedRecipeValidator()
        {
            RuleFor(v => v.Command.RecipeName)
                .NotEmpty().WithMessage("recipename field is required");
        }
    }
}
