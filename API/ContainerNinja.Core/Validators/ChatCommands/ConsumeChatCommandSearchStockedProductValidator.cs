using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchStockedProductValidator : AbstractValidator<ConsumeChatCommandSearchStockedProducts>
    {
        public ConsumeChatCommandSearchStockedProductValidator()
        {
            RuleFor(v => v.Command.Search).NotEmpty().WithMessage("Search is required");
        }
    }
}
