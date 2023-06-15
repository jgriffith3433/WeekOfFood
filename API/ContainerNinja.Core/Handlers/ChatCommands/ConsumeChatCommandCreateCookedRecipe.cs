using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "log_recipe" })]
    [ChatCommandSpecification("log_recipe", "Create a log of a recipe that was used.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""recipename"": {
            ""type"": ""string"",
            ""description"": ""The name of the recipe that was used.""
        }
    },
    ""required"": [""recipename""]
}")]
    public class ConsumeChatCommandCreateCookedRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateCookedRecipe>
    {
        public ChatAICommandDTOCreateCookedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateCookedRecipeHandler : IRequestHandler<ConsumeChatCommandCreateCookedRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateCookedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateCookedRecipe model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock)
                .Include(r => r.CookedRecipes)
                .Where(r => r.Name.ToLower() == model.Command.RecipeName.ToLower())
                .SingleOrDefaultAsync(cancellationToken).Result;

            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.RecipeName;
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
            return $"Created logged recipe: {model.Command.RecipeName}";
        }
    }
}