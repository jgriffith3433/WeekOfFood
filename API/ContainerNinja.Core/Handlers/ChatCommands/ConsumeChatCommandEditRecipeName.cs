using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "edit_recipe_name" })]
    public class ConsumeChatCommandEditRecipeName : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOEditRecipeName>
    {
        public ChatAICommandDTOEditRecipeName Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditRecipeNameHandler : IRequestHandler<ConsumeChatCommandEditRecipeName, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditRecipeNameHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandEditRecipeName model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            var searchTerms = string.Join(' ', model.Command.Original.ToLower().Split('-')).Split(' ');
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
                var systemResponse = "Error: Could not find recipe by name: " + model.Command.Original;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.Original.ToLower())
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Error: Could not find recipe by name '" + model.Command.Original + "'. Did you mean: " + query[0].Name + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.Original.ToLower());
                if (exactMatch != null)
                {
                    //exact match
                    recipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Error: Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (recipe != null)
            {
                var existingRecipeWithName = _repository.Recipes.FirstOrDefault(r => r.Name.ToLower() == model.Command.New.ToLower());
                if (existingRecipeWithName != null)
                {
                    var systemResponse = "Error: Recipe already exists: " + model.Command.New;
                    throw new ChatAIException(systemResponse);
                }

                recipe.Name = model.Command.New;
                _repository.Recipes.Update(recipe);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}