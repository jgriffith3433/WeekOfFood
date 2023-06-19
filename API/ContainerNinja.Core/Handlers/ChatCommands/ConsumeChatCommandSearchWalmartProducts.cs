using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_walmart_products" })]
    public class ConsumeChatCommandSearchWalmartProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchWalmartProducts>
    {
        public ChatAICommandDTOSearchWalmartProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchWalmartProductsHandler : IRequestHandler<ConsumeChatCommandSearchWalmartProducts, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandSearchWalmartProductsHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchWalmartProducts model, CancellationToken cancellationToken)
        {
            var walmartResults = await _walmartService.Search(model.Command.Search);
            if (walmartResults == null || walmartResults.items.Count == 0)
            {
                return $"There are no walmart products matching that search query.";
            }

            var walmartProducts = new JArray();
            foreach (var walmartItem in walmartResults.items)
            {
                var walmartProductObject = new JObject();
                walmartProductObject["WalmartId"] = walmartItem.itemId;
                walmartProductObject["WalmartProductName"] = walmartItem.name;
                walmartProducts.Add(walmartProductObject);
            }
            model.Response.NavigateToPage = "products";
            return "Walmart Products:\n" + JsonConvert.SerializeObject(walmartProducts);
        }
    }
}