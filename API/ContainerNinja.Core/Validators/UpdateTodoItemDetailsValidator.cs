using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateTodoItemDetailsValidator : AbstractValidator<UpdateTodoItemDetailsCommand>
    {
        public UpdateTodoItemDetailsValidator()
        {
            RuleFor(x => x.ListId).NotEmpty().WithMessage("ListId is required");
            RuleFor(x => x.Priority).NotEmpty().WithMessage("Priority is required");
        }
    }
}
