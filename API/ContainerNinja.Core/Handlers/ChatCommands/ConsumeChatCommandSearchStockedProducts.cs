using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_stocked_products" })]
    public class ConsumeChatCommandSearchStockedProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchStockedProducts>
    {
        public ChatAICommandDTOSearchStockedProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchProductStockHandler : IRequestHandler<ConsumeChatCommandSearchStockedProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSearchProductStockHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchStockedProducts model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<ProductStock>();
            if (string.IsNullOrEmpty(model.Command.Search))
            {
                predicate = predicate.Or(r => true);
            }
            else
            {
                var searchTerms = string.Join(' ', model.Command.Search.ToLower().Split('-')).Split(' ');
                foreach (var searchTerm in searchTerms)
                {
                    predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm));
                }
            }
            var query = _repository.ProductStocks.Set.AsExpandable().Where(predicate).ToList();
            var results = new JArray();
            foreach (var productStock in query)
            {
                var productStockObject = new JObject();
                productStockObject["StockedProductId"] = productStock.Id;
                productStockObject["StockedProductName"] = productStock.Name;
                results.Add(productStockObject);
            }
            if (results.Count == 0)
            {
                return $"No Product Stock matching the search term: {model.Command.Search}";
            }
            model.Response.NavigateToPage = "product-stocks";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(results);
        }
    }
}