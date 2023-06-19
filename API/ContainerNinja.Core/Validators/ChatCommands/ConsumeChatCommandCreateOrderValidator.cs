using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateOrderValidator : AbstractValidator<ConsumeChatCommandCreateOrder>
    {
        public ConsumeChatCommandCreateOrderValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Products).NotEmpty().WithMessage("Products is required");
            RuleForEach(v => v.Command.Products).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductId).NotEmpty().WithMessage("StockedProductId is required");
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required");
            });
        }
    }
}
