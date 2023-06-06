using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "delete_recipe" })]
    public class ConsumeChatCommandDeleteRecipe : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTODeleteRecipe>
    {
        public ChatAICommandDTODeleteRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandDeleteRecipe model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients)
                .Where(r => r.Name.ToLower() == model.Command.Name.ToLower())
                .SingleOrDefaultAsync(cancellationToken).Result;

            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + model.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipes = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(cr => cr.CookedRecipeCalledIngredients).Where(cr => cr.Recipe == recipe);
                if (cookedRecipes.Any())
                {
                    var systemResponse = "Error: Can not delete recipe because there are cooked recipe records that use the recipe: " + model.Command.Name;
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
            return model.Response;
        }
    }
}