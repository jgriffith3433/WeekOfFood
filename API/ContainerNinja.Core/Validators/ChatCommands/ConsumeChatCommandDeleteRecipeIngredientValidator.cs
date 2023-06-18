using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandDeleteRecipeIngredient>
    {
        public ConsumeChatCommandDeleteRecipeIngredientValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName is required");
        }
    }
}
