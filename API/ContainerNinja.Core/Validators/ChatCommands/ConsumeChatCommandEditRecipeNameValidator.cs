using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeNameValidator : AbstractValidator<ConsumeChatCommandEditRecipeName>
    {
        public ConsumeChatCommandEditRecipeNameValidator()
        {
            RuleFor(v => v.Command.OriginalName).NotEmpty().WithMessage("OriginalName is required");
            RuleFor(v => v.Command.NewName).NotEmpty().WithMessage("NewName is required");
        }
    }
}
