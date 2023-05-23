using ContainerNinja.Contracts.DTO;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateOrUpdateTodoListDTOValidator : AbstractValidator<CreateOrUpdateTodoListDTO>
    {
        public CreateOrUpdateTodoListDTOValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
            RuleFor(x => x.Color).NotEmpty().WithMessage("Tag a colorCode to the TodoList");
        }
    }
}
