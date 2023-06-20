using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteProductValidator : AbstractValidator<ConsumeChatCommandDeleteStockedProduct>
    {
        public ConsumeChatCommandDeleteProductValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.StockedProductId).NotEmpty().WithMessage("StockedProductId is required");
        }
    }
}
