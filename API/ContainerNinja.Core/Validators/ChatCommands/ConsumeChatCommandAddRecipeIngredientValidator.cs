using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandUpdateRecipe>
    {
        public ConsumeChatCommandAddRecipeIngredientValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId field is required");
            RuleFor(v => v.Command.Ingredients).NotEmpty().WithMessage("Ingredients field is required");
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName field is required");
            RuleFor(v => v.Command.Serves).NotEmpty().WithMessage("Serves field is required");
            RuleForEach(v => v.Command.Ingredients).ChildRules(i =>
            {
                i.RuleFor(x => x.IngredientName).NotEmpty().WithMessage("IngredientName field is required");
                i.RuleFor(x => x.Units).NotEmpty().WithMessage("Units field is required");
                i.RuleFor(x => x.KitchenUnitType).NotEmpty().WithMessage("KitchenUnitType field is required");
            });
        }
    }
}
