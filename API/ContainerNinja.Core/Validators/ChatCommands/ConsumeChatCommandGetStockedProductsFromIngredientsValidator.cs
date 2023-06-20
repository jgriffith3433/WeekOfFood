using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandGetStockedProductsFromIngredientsValidator : AbstractValidator<ConsumeChatCommandGetStockedProductsFromIngredients>
    {
        public ConsumeChatCommandGetStockedProductsFromIngredientsValidator()
        {
            var invalidIngredientIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "get_recipe_ingredients" });
            RuleFor(v => v.Command.IngredientIds).NotEmpty().WithMessage(invalidIngredientIdMessage);
        }
    }
}
