using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditProductUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditProductUnitType>
    {
        public ConsumeChatCommandEditProductUnitTypeValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.ProductName).NotEmpty().WithMessage("ProductName is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType is required");
        }
    }
}
