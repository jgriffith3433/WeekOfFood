using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDefaultValidator : AbstractValidator<ConsumeChatCommandUnknown>
    {
        public ConsumeChatCommandDefaultValidator()
        {
            RuleFor(v => v.Command.Cmd)
                .NotEmpty().WithMessage("cmd is required.");
        }
    }
}
