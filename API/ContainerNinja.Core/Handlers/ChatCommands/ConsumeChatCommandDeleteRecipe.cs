using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using LinqKit;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_recipe" })]
    [ChatCommandSpecification("delete_recipe", "Delete a recipe.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""name"": {
            ""type"": ""string"",
            ""description"": ""The name of the recipe to delete.""
        }
    },
    ""required"": [""name""]
}")]
    public class ConsumeChatCommandDeleteRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteRecipe>
    {
        public ChatAICommandDTODeleteRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteRecipe model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            var searchTerms = string.Join(' ', model.Command.Name.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients)
                .AsNoTracking()
                .AsExpandable()
                .Where(predicate).ToList();

            Recipe recipe;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.Name.ToLower())
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find recipe by name '" + model.Command.Name + "'. Did you mean: " + query[0].Name + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.Name.ToLower());
                if (exactMatch != null)
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (recipe != null)
            {
                var cookedRecipes = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(cr => cr.CookedRecipeCalledIngredients).Where(cr => cr.Recipe == recipe);
                if (cookedRecipes.Any())
                {
                    var systemResponse = "Can not delete recipe because there are logged recipe records that use the recipe: " + model.Command.Name;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    foreach (var calledIngredient in recipe.CalledIngredients)
                    {
                        _repository.CalledIngredients.Delete(calledIngredient.Id);
                    }
                    _repository.Recipes.Delete(recipe.Id);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}