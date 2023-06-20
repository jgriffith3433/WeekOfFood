using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandLinkRecipeIngredientToStockedProductValidator : AbstractValidator<ConsumeChatCommandLinkRecipeIngredientToStockedProduct>
    {
        public ConsumeChatCommandLinkRecipeIngredientToStockedProductValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");

            var invalidRecipeIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_recipes" });
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(1).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(12).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(123).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(1234).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(12345).WithMessage(invalidRecipeIdMessage);

            var invalidStockedProductIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "get_stocked_product_id" });
            RuleFor(v => v.Command.StockedProductId).NotEmpty().WithMessage(invalidStockedProductIdMessage);
            RuleFor(v => v.Command.StockedProductId).NotEqual(1).WithMessage(invalidStockedProductIdMessage);
            RuleFor(v => v.Command.StockedProductId).NotEqual(12).WithMessage(invalidStockedProductIdMessage);
            RuleFor(v => v.Command.StockedProductId).NotEqual(123).WithMessage(invalidStockedProductIdMessage);
            RuleFor(v => v.Command.StockedProductId).NotEqual(1234).WithMessage(invalidStockedProductIdMessage);
            RuleFor(v => v.Command.StockedProductId).NotEqual(12345).WithMessage(invalidStockedProductIdMessage);

            var invalidIngredientIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "get_recipe_ingredients" });
            RuleFor(v => v.Command.IngredientId).NotEmpty().WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(1).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(12).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(123).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(1234).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(12345).WithMessage(invalidIngredientIdMessage);
        }
    }
}
