using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateCookedRecipeValidator : AbstractValidator<ConsumeChatCommandCreateCookedRecipe>
    {
        public ConsumeChatCommandCreateCookedRecipeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId is required");
            RuleFor(v => v.Command.When).NotEmpty().WithMessage("When is required");
        }
    }
}
