using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateCompletedOrderCommandValidator : AbstractValidator<UpdateCompletedOrderCommand>
    {
        public UpdateCompletedOrderCommandValidator()
        {
            RuleFor(v => v.UserImport)
                .NotEmpty().WithMessage("UserImport is required.")
                .MaximumLength(40000).WithMessage("UserImport must not exceed 40000 characters.");
        }
    }
}
