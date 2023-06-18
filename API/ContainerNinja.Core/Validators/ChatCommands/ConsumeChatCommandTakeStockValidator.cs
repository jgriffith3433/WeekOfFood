using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandTakeStockValidator : AbstractValidator<ConsumeChatCommandUpdateProductStock>
    {
        public ConsumeChatCommandTakeStockValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.StockedProducts).NotEmpty().WithMessage("Items is required");
            RuleForEach(v => v.Command.StockedProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductName).NotEmpty().WithMessage("ItemName is required");
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required");
                i.RuleFor(x => x.UnitType).NotEmpty().WithMessage("UnitType is required");
            });
        }
    }
}
