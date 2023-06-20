using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteWalmartProductValidator : AbstractValidator<ConsumeChatCommandDeleteStockedProduct>
    {
        public ConsumeChatCommandDeleteWalmartProductValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.StockedProductId).NotEmpty().WithMessage("StockedProductId field is required");
        }
    }
}
