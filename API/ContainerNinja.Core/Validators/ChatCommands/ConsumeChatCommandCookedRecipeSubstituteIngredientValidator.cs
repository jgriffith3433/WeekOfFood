using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCookedRecipeSubstituteIngredientValidator : AbstractValidator<ConsumeChatCommandSubstituteCookedRecipeIngredient>
    {
        public ConsumeChatCommandCookedRecipeSubstituteIngredientValidator()
        {
            RuleFor(v => v.Command.Original)
                .NotEmpty().WithMessage("original field is required");
        }
    }
}
