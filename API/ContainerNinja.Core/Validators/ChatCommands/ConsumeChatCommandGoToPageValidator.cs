using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandGoToPageValidator : AbstractValidator<ConsumeChatCommandGoToPage>
    {
        public ConsumeChatCommandGoToPageValidator()
        {
            RuleFor(v => v.Command.Page)
                .NotEmpty().WithMessage("page field is required.");
        }
    }
}
