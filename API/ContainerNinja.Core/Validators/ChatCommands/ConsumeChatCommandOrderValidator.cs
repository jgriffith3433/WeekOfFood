using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandOrderValidator : AbstractValidator<ConsumeChatCommandOrder>
    {
        public ConsumeChatCommandOrderValidator()
        {
            RuleFor(v => v.Command.Items)
                .NotEmpty().WithMessage("items field must not be empty.");
        }
    }
}
