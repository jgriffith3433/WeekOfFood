using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateProductStockDetailsCommandValidator : AbstractValidator<UpdateProductStockDetailsCommand>
    {
        public UpdateProductStockDetailsCommandValidator()
        {
        }
    }
}
