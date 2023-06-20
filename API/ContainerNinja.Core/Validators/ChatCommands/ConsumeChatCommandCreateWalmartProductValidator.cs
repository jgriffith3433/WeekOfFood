using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateWalmartProductValidator : AbstractValidator<ConsumeChatCommandCreateWalmartProduct>
    {
        public ConsumeChatCommandCreateWalmartProductValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.ProductName).NotEmpty().WithMessage("ProductName field is required");
        }
    }
}
