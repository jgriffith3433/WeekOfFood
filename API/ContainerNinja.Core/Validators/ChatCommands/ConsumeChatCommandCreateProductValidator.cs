using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateProductValidator : AbstractValidator<ConsumeChatCommandCreateProduct>
    {
        public ConsumeChatCommandCreateProductValidator()
        {
            RuleFor(v => v.Command.ProductName).NotEmpty().WithMessage("ProductName is required");
        }
    }
}
