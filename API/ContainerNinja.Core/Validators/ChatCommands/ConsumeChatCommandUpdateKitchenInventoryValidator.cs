using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandUpdateKitchenInventoryValidator : AbstractValidator<ConsumeChatCommandUpdateKitchenInventory>
    {
        public ConsumeChatCommandUpdateKitchenInventoryValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            //RuleFor(v => v.Command.KitchenInventoryId).NotEmpty().WithMessage("KitchenInventoryId field is required");
            RuleFor(v => v.Command.KitchenProducts).NotEmpty().WithMessage("KitchenProducts field is required");
            RuleForEach(v => v.Command.KitchenProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductId).NotEmpty().WithMessage("KitchenProductId field is required");
                i.RuleFor(x => x.KitchenUnitType).NotEmpty().WithMessage($"KitchenUnitType field is required. The available values are: {string.Join(", ", Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().Select(p => p.ToString()))}");
            });
        }
    }
}
