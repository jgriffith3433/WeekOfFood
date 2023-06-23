using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateStockedProductsValidator : AbstractValidator<ConsumeChatCommandCreateStockedProducts>
    {
        public ConsumeChatCommandCreateStockedProductsValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.KitchenProducts).NotEmpty().WithMessage("StockedProducts field is required");
            RuleForEach(v => v.Command.KitchenProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductName).NotEmpty().WithMessage("StockedProductName field is required");
                i.RuleFor(x => x.KitchenUnitType).NotEmpty().WithMessage("UnitType field is required");
            });
        }
    }
}
