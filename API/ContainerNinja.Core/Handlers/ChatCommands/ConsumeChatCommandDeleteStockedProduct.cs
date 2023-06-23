using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_stocked_products" })]
    public class ConsumeChatCommandDeleteStockedProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteStockedProducts>
    {
        public ChatAICommandDTODeleteStockedProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteStockedProductHandler : IRequestHandler<ConsumeChatCommandDeleteStockedProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteStockedProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteStockedProducts model, CancellationToken cancellationToken)
        {
            foreach (var productToDelete in model.Command.StockedProductsToDelete)
            {
                var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Id == productToDelete.StockedProductId);
                if (productStockEntity == null)
                {
                    var systemResponse = "Could not find stocked product by ID: " + productToDelete.StockedProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_stocked_products"" }");
                }
                if (productStockEntity != null)
                {
                    var calledIngredients = _repository.CalledIngredients.Set.Where(ci => ci.ProductStock != null && ci.ProductStock == productStockEntity);
                    foreach (var calledIngredient in calledIngredients)
                    {
                        calledIngredient.ProductStock = null;
                        _repository.CalledIngredients.Update(calledIngredient);
                    }
                    var cookedRecipeCalledIngredients = _repository.CookedRecipeCalledIngredients.Set.Where(ci => ci.ProductStock != null && ci.ProductStock == productStockEntity);
                    foreach (var cookedRecipeCalledIngredient in cookedRecipeCalledIngredients)
                    {
                        cookedRecipeCalledIngredient.ProductStock = null;
                        _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                    }

                    _repository.ProductStocks.Delete(productStockEntity.Id);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "product-stocks";
            return "Success";
        }
    }
}