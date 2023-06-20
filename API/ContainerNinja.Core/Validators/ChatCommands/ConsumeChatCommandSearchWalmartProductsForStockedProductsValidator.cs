using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchWalmartProductsForStockedProductsValidator : AbstractValidator<ConsumeChatCommandSearchWalmartProductsForStockedProduct>
    {
        public ConsumeChatCommandSearchWalmartProductsForStockedProductsValidator()
        {
            RuleFor(v => v.Command.StockedProductIds).NotEmpty().WithMessage("StockedProductIds is required");
        }
    }
}
