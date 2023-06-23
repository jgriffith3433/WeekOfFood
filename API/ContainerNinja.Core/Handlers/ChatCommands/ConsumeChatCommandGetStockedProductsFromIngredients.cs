using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_stocked_products_from_ingredients" })]
    public class ConsumeChatCommandGetStockedProductsFromIngredients : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetStockedProductsFromIngredients>
    {
        public ChatAICommandDTOGetStockedProductsFromIngredients Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetStockedProductsFromIngredientsHandler : IRequestHandler<ConsumeChatCommandGetStockedProductsFromIngredients, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetStockedProductsFromIngredientsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetStockedProductsFromIngredients model, CancellationToken cancellationToken)
        {
            var stockedProductsArray = new JArray();
            foreach (var id in model.Command.IngredientIds)
            {
                var ingredientEntity = _repository.CalledIngredients.Set.FirstOrDefault(ci => ci.Id == id);
                if (ingredientEntity == null)
                {
                    var systemResponse = "Could not find ingredient by ID: " + id;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""get_recipe_ingredients"" }");
                }
                var stockedProductLinkObject = new JObject();
                stockedProductLinkObject["IngredientId"] = ingredientEntity.Id;
                stockedProductLinkObject["StockedProductId"] = ingredientEntity.ProductStock?.Id;
                stockedProductLinkObject["StockedProductKitchenUnits"] = ingredientEntity.ProductStock?.Units;
                stockedProductLinkObject["StockedProductKitchenUnitType"] = ingredientEntity.UnitType.ToString();
                stockedProductsArray.Add(stockedProductLinkObject);
            }

            model.Response.NavigateToPage = "recipes";
            return JsonConvert.SerializeObject(stockedProductsArray, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}