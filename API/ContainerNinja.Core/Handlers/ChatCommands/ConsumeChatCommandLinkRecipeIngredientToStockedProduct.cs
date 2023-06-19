using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "link_recipe_ingredient_to_stocked_product" })]
    public class ConsumeChatCommandLinkRecipeIngredientToStockedProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOLinkRecipeIngredientToStockedProduct>
    {
        public ChatAICommandDTOLinkRecipeIngredientToStockedProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandLinkRecipeIngredientToStockedProductHandler : IRequestHandler<ConsumeChatCommandLinkRecipeIngredientToStockedProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandLinkRecipeIngredientToStockedProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandLinkRecipeIngredientToStockedProduct model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_recipe_id"" }");
            }

            var ingredientEntity = recipeEntity.CalledIngredients.FirstOrDefault(i => i.Id == model.Command.IngredientId);
            if (ingredientEntity == null)
            {
                var systemResponse = "Could not find ingredient by ID: " + model.Command.IngredientId;
                throw new ChatAIException(systemResponse);
            }

            var stockedProductEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Id == model.Command.StockedProductId);
            if (stockedProductEntity == null)
            {
                var systemResponse = "Could not find stocked product by ID: " + model.Command.StockedProductId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_stocked_product_id"" }");
            }

            ingredientEntity.ProductStock = stockedProductEntity;
            _repository.CalledIngredients.Update(ingredientEntity);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "recipes";
            return $"Successfully linked IngredientId: {model.Command.IngredientId} to StockedProductId: {model.Command.StockedProductId}";
        }
    }
}