using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchStockedProductValidator : AbstractValidator<ConsumeChatCommandSearchStockedProducts>
    {
        public ConsumeChatCommandSearchStockedProductValidator()
        {
            RuleFor(v => v.Command.StockedProductName).NotEmpty().WithMessage("Search field is required");
        }
    }
}
