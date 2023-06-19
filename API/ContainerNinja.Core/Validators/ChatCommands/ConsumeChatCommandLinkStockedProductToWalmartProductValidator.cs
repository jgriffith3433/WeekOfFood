using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandLinkStockedProductToWalmartProductValidator : AbstractValidator<ConsumeChatCommandLinkStockedProductToWalmartProduct>
    {
        public ConsumeChatCommandLinkStockedProductToWalmartProductValidator()
        {
            var forceFunctionCallObject = new { name = "search_walmart_products" };
            var invalidWalmartProductIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(forceFunctionCallObject);
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.StockedProductId).NotEmpty().WithMessage("StockedProductId is required");
            RuleFor(v => v.Command.WalmartProductId).NotEmpty().WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(1).WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(1234).WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(12345).WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(123456).WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(1234567).WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(12345678).WithMessage(invalidWalmartProductIdMessage);
            RuleFor(v => v.Command.WalmartProductId).NotEqual(123456789).WithMessage(invalidWalmartProductIdMessage);
        }
    }
}
