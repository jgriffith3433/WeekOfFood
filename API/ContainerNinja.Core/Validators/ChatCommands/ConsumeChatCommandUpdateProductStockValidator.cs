using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandUpdateStockedProductValidator : AbstractValidator<ConsumeChatCommandUpdateStockedProducts>
    {
        public ConsumeChatCommandUpdateStockedProductValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.StockedProducts).NotEmpty().WithMessage("StockedProducts field is required");
            RuleForEach(v => v.Command.StockedProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductName).NotEmpty().WithMessage("StockedProductName field is required");
                i.RuleFor(x => x.UnitType).NotEmpty().WithMessage("UnitType field is required");
            });
        }
    }
}
