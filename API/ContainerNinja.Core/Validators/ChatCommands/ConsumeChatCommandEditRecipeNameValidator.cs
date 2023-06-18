using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeNameValidator : AbstractValidator<ConsumeChatCommandEditRecipeName>
    {
        public ConsumeChatCommandEditRecipeNameValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(v => v.Command.NewName).NotEmpty().WithMessage("NewName is required");
        }
    }
}
