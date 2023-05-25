using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateProductStockCommandValidator : AbstractValidator<CreateProductStockCommand>
    {
        public CreateProductStockCommandValidator()
        {
            RuleFor(x => x.Name).MaximumLength(200).NotEmpty();
        }
    }
}
