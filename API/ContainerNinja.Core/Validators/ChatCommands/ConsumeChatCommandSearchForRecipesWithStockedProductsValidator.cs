using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchForRecipesWithStockedProductsValidator : AbstractValidator<ConsumeChatCommandSearchForRecipesWithStockedProducts>
    {
        public ConsumeChatCommandSearchForRecipesWithStockedProductsValidator()
        {
            RuleFor(v => v.Command.StockedProducts).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "get_stocked_products" }));
            RuleForEach(v => v.Command.StockedProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductId).NotEmpty().WithMessage("StockedProductId field is required");
            });
        }
    }
}
