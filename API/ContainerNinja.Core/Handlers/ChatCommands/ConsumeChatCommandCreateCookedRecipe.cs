using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_cooked_recipe" })]
    public class ConsumeChatCommandCreateCookedRecipe : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOCreateCookedRecipe>
    {
        public ChatAICommandDTOCreateCookedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateCookedRecipeHandler : IRequestHandler<ConsumeChatCommandCreateCookedRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateCookedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCreateCookedRecipe model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock)
                .Include(r => r.CookedRecipes)
                .Where(r => r.Name.ToLower() == model.Command.Name.ToLower())
                .SingleOrDefaultAsync(cancellationToken).Result;

            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + model.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipe = new CookedRecipe
                {
                    Recipe = recipe
                };
                foreach (var calledIngredient in recipe.CalledIngredients)
                {
                    var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                    {
                        Name = calledIngredient.Name,
                        CookedRecipe = cookedRecipe,
                        CalledIngredient = calledIngredient,
                        ProductStock = calledIngredient.ProductStock,
                        UnitType = calledIngredient.UnitType,
                        Units = calledIngredient.Units != null ? calledIngredient.Units.Value : 0
                    };
                    cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                }
                recipe.CookedRecipes.Add(cookedRecipe);
                _repository.Recipes.Update(recipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}