using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditWalmartProductUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditWalmartProductUnitType>
    {
        public ConsumeChatCommandEditWalmartProductUnitTypeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.ProductId).NotEmpty().WithMessage("ProductId field is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType field is required");
        }
    }
}
