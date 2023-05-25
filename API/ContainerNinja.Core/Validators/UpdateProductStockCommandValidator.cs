using ContainerNinja.Contracts.DTO;
using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
    {
        public UpdateProductStockCommandValidator()
        {
            //RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
            //RuleFor(x => x.Color).NotEmpty().WithMessage("Tag a colorCode to the TodoList");
        }
    }
}
