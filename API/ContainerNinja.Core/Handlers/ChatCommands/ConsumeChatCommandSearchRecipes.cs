using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "search_recipes" })]
    [ChatCommandSpecification("search_recipes", "Searches for recipes by name.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""search"": {
            ""type"": ""string"",
            ""description"": ""The term to search by for recipe names.""
        }
    },
    ""required"": [""search""]
}")]
    public class ConsumeChatCommandSearchRecipes : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchRecipes>
    {
        public ChatAICommandDTOSearchRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGoToRecipesHandler : IRequestHandler<ConsumeChatCommandSearchRecipes, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGoToRecipesHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchRecipes model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            var searchTerms = string.Join(' ', model.Command.Search.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients)
                .AsNoTracking()
                .AsExpandable()
                .Where(predicate).ToList();

            return "Results: " + string.Join(", ", query.Select(r => r.Name));
        }
    }
}