using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditCookedRecipeDateValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeDate>
    {
        public ConsumeChatCommandEditCookedRecipeDateValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId is required");
            RuleFor(v => v.Command.When).NotEmpty().WithMessage("When is required");
        }
    }
}
