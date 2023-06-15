using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDefaultValidator : AbstractValidator<ConsumeChatCommandUnknown>
    {
        public ConsumeChatCommandDefaultValidator()
        {
        }
    }
}
