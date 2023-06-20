using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddStockedProductsToOrderValidator : AbstractValidator<ConsumeChatCommandAddStockedProductsToOrder>
    {
        public ConsumeChatCommandAddStockedProductsToOrderValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.OrderId).NotEmpty().WithMessage("OrderId is required");
            RuleFor(v => v.Command.StockedProducts).NotEmpty().WithMessage("Products is required");
            RuleForEach(v => v.Command.StockedProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductId).NotEmpty().WithMessage("StockedProductId is required");
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required");
            });
        }
    }
}
