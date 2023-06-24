using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json.Linq;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_walmart_products_for_kitchen_product" })]
    public class ConsumeChatCommandSearchWalmartProductsForKitchenProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchWalmartProductsForKitchenProduct>
    {
        public ChatAICommandDTOSearchWalmartProductsForKitchenProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchWalmartProductsForKitchenProductHandler : IRequestHandler<ConsumeChatCommandSearchWalmartProductsForKitchenProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandSearchWalmartProductsForKitchenProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchWalmartProductsForKitchenProduct model, CancellationToken cancellationToken)
        {
            var kitchenProductsToFindWalmartProducts = new List<KitchenProduct>();
            var kitchenProductWalmartProducts = new JArray();
            foreach(var id in model.Command.KitchenProductIds)
            {
                var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Id == id);
                if (kitchenProductEntity == null)
                {
                    var systemResponse = "Could not find kitchen product by ID: " + id;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_kitchen_products"" }");
                }
                var walmartProductsArray = new JArray();
                var walmartSearchResults = await _walmartService.Search(kitchenProductEntity.Name);
                if (walmartSearchResults != null && walmartSearchResults.items != null)
                {
                    foreach (var walmartItem in walmartSearchResults.items)
                    {
                        var walmartProductObject = new JObject();
                        walmartProductObject["WalmartId"] = walmartItem.itemId;
                        walmartProductObject["WalmartProductName"] = walmartItem.name;
                        walmartProductObject["WalmartProductSize"] = walmartItem.size;
                        walmartProductsArray.Add(walmartProductObject);
                    }
                }
                var kitchenProductWalmartProductsObject = new JObject();
                kitchenProductWalmartProductsObject["KitchenProductId"] = kitchenProductEntity.Id;
                kitchenProductWalmartProductsObject["WalmartSearchResults"] = walmartProductsArray;
                kitchenProductWalmartProducts.Add(kitchenProductWalmartProductsObject);
            }
            model.Response.ForceFunctionCall = "none";
            model.Response.NavigateToPage = "products";
            return JsonConvert.SerializeObject(kitchenProductWalmartProducts, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}