using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandOrderValidator : AbstractValidator<ConsumeChatCommandOrder>
    {
        public ConsumeChatCommandOrderValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Products).NotEmpty().WithMessage("Products is required");
            RuleForEach(v => v.Command.Products).ChildRules(i =>
            {
                i.RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required");
            });
        }
    }
}
