using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchWalmartProductsForKitchenProductsValidator : AbstractValidator<ConsumeChatCommandSearchWalmartProductsForKitchenProduct>
    {
        public ConsumeChatCommandSearchWalmartProductsForKitchenProductsValidator()
        {
            RuleFor(v => v.Command.KitchenProductIds).NotEmpty().WithMessage("KitchenProductIds field is required");
        }
    }
}
