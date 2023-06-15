using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "add_recipe_ingredient" })]
    public class ConsumeChatCommandAddRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddRecipeIngredient>
    {
        public ChatAICommandDTOAddRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddRecipeIngredient model, CancellationToken cancellationToken)
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
                var calledIngredient = _repository.CalledIngredients.CreateProxy();
                {
                    calledIngredient.Name = model.Command.IngredientName;
                    calledIngredient.Recipe = recipe;
                    calledIngredient.Verified = false;
                    calledIngredient.Units = model.Command.Units;
                    calledIngredient.UnitType = model.Command.UnitType.UnitTypeFromString();
                };
                recipe.CalledIngredients.Add(calledIngredient);
                _repository.Recipes.Update(recipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}