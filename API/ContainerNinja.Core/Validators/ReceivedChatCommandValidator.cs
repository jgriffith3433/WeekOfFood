using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class ConsumeChatCommandValidator : AbstractValidator<ConsumeChatCommand>
    {
        public ConsumeChatCommandValidator()
        {
        }
    }
}
