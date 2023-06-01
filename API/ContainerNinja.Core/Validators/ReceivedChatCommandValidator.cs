using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class ReceivedChatCommandValidator : AbstractValidator<ReceivedChatCommand>
    {
        public ReceivedChatCommandValidator()
        {
        }
    }
}
