using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_kitchen_products" })]
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
            var listOfResults = new List<List<ProductStock>>();
            foreach (var name in model.Command.ListOfNames)
            {
                var predicate = PredicateBuilder.New<ProductStock>();
                if (string.IsNullOrEmpty(name.StockedProductName) || name.StockedProductName == "*" || name.StockedProductName == "stock" || name.StockedProductName == "all")
                {
                    predicate = predicate.Or(r => true);
                }
                else
                {
                    var searchTerms = string.Join(' ', name.StockedProductName.ToLower().Split('-')).Split(' ');
                    foreach (var searchTerm in searchTerms)
                    {
                        predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm));
                        if (searchTerm[searchTerm.Length - 1] == 's')
                        {
                            predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm.Substring(0, searchTerm.Length - 1)));
                        }
                    }
                }
                listOfResults.Add(_repository.ProductStocks.Set.AsExpandable().Where(predicate).ToList());
            }
            var allSearchResultsObject = new JObject();
            for (var i = 0; i < model.Command.ListOfNames.Count; i++)
            {
                var results = listOfResults[i];
                var searchResultsObject = new JObject
                {
                    { "Message", $"{results.Count} records found for {model.Command.ListOfNames[i].StockedProductName}" }
                };
                if (results.Count > 0)
                {
                    var foundArray = new JArray();
                    foreach (var productStock in results)
                    {
                        var foundObject = new JObject();
                        foundObject["KitchenProductId"] = productStock.Id;
                        foundObject["KitchenProductName"] = productStock.Name;
                        foundArray.Add(foundObject);
                    }
                    searchResultsObject.Add("Results", foundArray);
                }
                allSearchResultsObject.Add(model.Command.ListOfNames[i].StockedProductName, searchResultsObject);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            model.Response.NavigateToPage = "product-stocks";
            return JsonConvert.SerializeObject(allSearchResultsObject, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}