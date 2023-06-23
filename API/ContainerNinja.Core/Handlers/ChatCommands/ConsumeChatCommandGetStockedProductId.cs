using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerNinja.Contracts.Walmart;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    //[ChatCommandModel(new[] { "search_stocked_products" })]
    public class ConsumeChatCommandGetStockedProductId : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetStockedProductId>
    {
        public ChatAICommandDTOGetStockedProductId Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetStockedProductIdHandler : IRequestHandler<ConsumeChatCommandGetStockedProductId, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetStockedProductIdHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetStockedProductId model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<ProductStock>();
            var searchTerms = string.Join(' ', model.Command.StockedProductName.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
                if (searchTerm[searchTerm.Length - 1] == 's')
                {
                    predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm.Substring(0, searchTerm.Length - 1)));
                }
            }

            var query = _repository.ProductStocks.Set.AsExpandable().Where(predicate).ToList();

            ProductStock productStock;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find stocked product by name: " + model.Command.StockedProductName;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.StockedProductName.ToLower())
                {
                    //exact match
                    productStock = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find stocked product by name '" + model.Command.StockedProductName + "'. Did you mean: '" + query[0].Name + "' with ID: " + query[0].Id + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(ps => ps.Name.ToLower() == model.Command.StockedProductName.ToLower());

                if (exactMatch != null)
                {
                    //check if there are no "too similar" matches
                    //for instance: "trail mix" could be "raisin free trail mix" or "trail mix snack"
                    var tooSimilarMatch = query.FirstOrDefault(ps => ps != exactMatch && ps.Name.ToLower().Contains(model.Command.StockedProductName.ToLower()));
                    if (tooSimilarMatch != null)
                    {
                        exactMatch = null;
                    }
                }

                if (exactMatch != null)
                {
                    //exact match
                    productStock = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (productStock == null)
            {
                var systemResponse = "Could not find stocked product by name '" + model.Command.StockedProductName + "'.";
                throw new ChatAIException(systemResponse);
            }
            var productStockObject = new JObject();
            productStockObject["StockedProductId"] = productStock.Id;
            productStockObject["StockedProductName"] = productStock.Name;
            model.Response.ForceFunctionCall = "auto";
            model.Response.NavigateToPage = "product-stocks";
            return JsonConvert.SerializeObject(productStockObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}