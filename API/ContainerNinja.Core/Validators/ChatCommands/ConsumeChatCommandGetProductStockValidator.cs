using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandGetProductStockValidator : AbstractValidator<ConsumeChatCommandGetStockedProducts>
    {
        public ConsumeChatCommandGetProductStockValidator()
        {
        }
    }
}
