using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditProductUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditProductUnitType>
    {
        public ConsumeChatCommandEditProductUnitTypeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.ProductId).NotEmpty().WithMessage("ProductId is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType is required");
        }
    }
}
