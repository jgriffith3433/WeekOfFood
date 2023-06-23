//using ContainerNinja.Contracts.Enum;
//using ContainerNinja.Core.Handlers.ChatCommands;
//using FluentValidation;

//namespace ContainerNinja.Core.Validators.ChatCommands
//{
//    public class ConsumeChatCommandUpdateStockedProductsValidator : AbstractValidator<ConsumeChatCommandUpdateStockedProducts>
//    {
//        public ConsumeChatCommandUpdateStockedProductsValidator()
//        {
//            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
//            RuleFor(v => v.Command.StockedProducts).NotEmpty().WithMessage("StockedProducts field is required");
//            RuleForEach(v => v.Command.StockedProducts).ChildRules(i =>
//            {
//                i.RuleFor(x => x.StockedProductId).NotEmpty().WithMessage("StockedProductId field is required");
//                i.RuleFor(x => x.KitchenUnitType).NotEmpty().WithMessage("UnitType field is required");
//            });
//        }
//    }
//}
