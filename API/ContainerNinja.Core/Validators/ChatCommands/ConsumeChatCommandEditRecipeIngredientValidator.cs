using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandEditRecipeIngredient>
    {
        public ConsumeChatCommandEditRecipeIngredientValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Id).NotEmpty().WithMessage("Id required");
            RuleFor(v => v.Command.OriginalIngredient).NotEmpty().WithMessage("OriginalIngredient required");
            RuleFor(v => v.Command.NewIngredient).NotEmpty().WithMessage("NewIngredient required");
        }
    }
}
