using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandTemplateValidator : AbstractValidator<ConsumeChatCommandTemplate>
    {
        public ConsumeChatCommandTemplateValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.TemplateProperty).NotEmpty().WithMessage("TemplateProperty required");
        }
    }
}
