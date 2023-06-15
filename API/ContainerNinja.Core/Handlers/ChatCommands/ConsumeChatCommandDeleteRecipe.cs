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
            var searchTerms = string.Join(' ', model.Command.RecipeName.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.Recipes.Set.AsExpandable().Where(predicate).ToList();

            Recipe recipe;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.RecipeName.ToLower())
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find recipe by name '" + model.Command.RecipeName + "'. Did you mean: " + query[0].Name + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.RecipeName.ToLower());
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
                var cookedRecipes = _repository.CookedRecipes.Set.Where(cr => cr.Recipe == recipe);
                if (cookedRecipes.Any())
                {
                    var systemResponse = "Can not delete recipe because there are logged recipe records that use the recipe: " + model.Command.RecipeName;
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