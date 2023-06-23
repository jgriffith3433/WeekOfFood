using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchStockedProductValidator : AbstractValidator<ConsumeChatCommandSearchStockedProducts>
    {
        public ConsumeChatCommandSearchStockedProductValidator()
        {
            RuleFor(v => v.Command.ListOfNames).NotEmpty().WithMessage("ListOfNames is required");
            RuleForEach(v => v.Command.ListOfNames).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductName).NotEmpty().WithMessage("StockedProductName field is required");
            });
        }
    }
}
