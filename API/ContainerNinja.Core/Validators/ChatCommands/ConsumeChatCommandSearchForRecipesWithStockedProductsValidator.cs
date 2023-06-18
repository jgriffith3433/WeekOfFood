using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchForRecipesWithStockedProductsValidator : AbstractValidator<ConsumeChatCommandSearchForRecipesWithStockedProducts>
    {
        public ConsumeChatCommandSearchForRecipesWithStockedProductsValidator()
        {
            var forceFunctionCallObject = new
            {
                name = "get_stocked_products"
            };
            RuleFor(v => v.Command.StockedProducts).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(forceFunctionCallObject));
            RuleForEach(v => v.Command.StockedProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.StockedProductId).NotEmpty().WithMessage("StockedProductId is required");
            });
        }
    }
}
