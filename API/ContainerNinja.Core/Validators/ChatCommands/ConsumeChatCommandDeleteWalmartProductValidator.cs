using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteStockedProductsValidator : AbstractValidator<ConsumeChatCommandDeleteStockedProducts>
    {
        public ConsumeChatCommandDeleteStockedProductsValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.StockedProductsToDelete).NotEmpty().WithMessage("StockedProductsToDelete field is required");
            RuleForEach(v => v.Command.StockedProductsToDelete).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductId).NotEmpty().WithMessage("StockedProductId field is required");
            });
        }
    }
}
