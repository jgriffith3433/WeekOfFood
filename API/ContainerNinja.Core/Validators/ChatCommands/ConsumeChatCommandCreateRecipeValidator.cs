using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateRecipeValidator : AbstractValidator<ConsumeChatCommandCreateRecipe>
    {
        public ConsumeChatCommandCreateRecipeValidator()
        {
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
            RuleFor(v => v.Command.Serves).NotEmpty().WithMessage("Serves is required");
            RuleFor(v => v.Command.Ingredients).NotEmpty().WithMessage("Ingredients is required");
            RuleForEach(v => v.Command.Ingredients).ChildRules(i =>
            {
                i.RuleFor(x => x.IngredientName).NotEmpty().WithMessage("IngredientName is required");
                i.RuleFor(x => x.Units).NotEmpty().WithMessage("Units is required");
                i.RuleFor(x => x.UnitType).NotEmpty().WithMessage("UnitType is required");
            });
        }
    }
}
