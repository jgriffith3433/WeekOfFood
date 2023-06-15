using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddTodoListValidator : AbstractValidator<ConsumeChatCommandAddTodoList>
    {
        public ConsumeChatCommandAddTodoListValidator()
        {
            RuleFor(v => v.Command.ListName).NotEmpty().WithMessage("ListName is required");
        }
    }
}
