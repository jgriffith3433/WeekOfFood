using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateRecipeValidator : AbstractValidator<ConsumeChatCommandCreateRecipe>
    {
        public ConsumeChatCommandCreateRecipeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName field is required");
            RuleFor(v => v.Command.Serves).NotEmpty().WithMessage("Serves field is required");
            RuleFor(v => v.Command.Ingredients).NotEmpty().WithMessage("Ingredients field is required");
            RuleForEach(v => v.Command.Ingredients).ChildRules(i =>
            {
                i.RuleFor(x => x.IngredientName).NotEmpty().WithMessage("IngredientName field is required");
                i.RuleFor(x => x.Units).NotEmpty().WithMessage("Units field is required");
                i.RuleFor(x => x.UnitType).NotEmpty().WithMessage("UnitType field is required");
            });
        }
    }
}
