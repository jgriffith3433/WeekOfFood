using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCookedRecipeSubstituteIngredientValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeIngredient>
    {
        public ConsumeChatCommandCookedRecipeSubstituteIngredientValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(v => v.Command.OriginalIngredient).NotEmpty().WithMessage("OriginalIngredient is required");
            RuleFor(v => v.Command.NewIngredient).NotEmpty().WithMessage("NewIngredient is required");
        }
    }
}
