using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeNameValidator : AbstractValidator<ConsumeChatCommandEditRecipeName>
    {
        public ConsumeChatCommandEditRecipeNameValidator()
        {
            RuleFor(v => v.Command.Original)
                .NotEmpty().WithMessage("original field is required.");

            RuleFor(v => v.Command.New)
                .NotEmpty().WithMessage("new field is required.");
        }
    }
}
