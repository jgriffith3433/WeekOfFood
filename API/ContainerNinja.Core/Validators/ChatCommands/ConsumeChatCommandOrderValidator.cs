using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandOrderValidator : AbstractValidator<ConsumeChatCommandOrder>
    {
        public ConsumeChatCommandOrderValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Items).NotEmpty().WithMessage("Items is required");
            RuleForEach(v => v.Command.Items).ChildRules(i =>
            {
                i.RuleFor(x => x.ItemName).NotEmpty().WithMessage("ItemName is required");
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required");
            });
        }
    }
}
