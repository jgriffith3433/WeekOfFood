using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteProductValidator : AbstractValidator<ConsumeChatCommandDeleteProduct>
    {
        public ConsumeChatCommandDeleteProductValidator()
        {
            RuleFor(v => v.Command.ProductName).NotEmpty().WithMessage("ProductName is required");
        }
    }
}
