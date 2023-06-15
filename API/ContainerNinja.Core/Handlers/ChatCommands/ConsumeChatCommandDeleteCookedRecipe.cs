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
    [ChatCommandModel(new [] { "delete_logged_recipe" })]
    [ChatCommandSpecification("delete_logged_recipe", "Delete a logged recipe.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""name"": {
            ""type"": ""string"",
            ""description"": ""The name of the logged recipe to delete.""
        }
    },
    ""required"": [""name""]
}")]
    public class ConsumeChatCommandDeleteCookedRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteCookedRecipe>
    {
        public ChatAICommandDTODeleteCookedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteCookedRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteCookedRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteCookedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteCookedRecipe model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(r => r.CookedRecipeCalledIngredients)
                .FirstOrDefault(cr => cr.Recipe.Name.ToLower() == model.Command.Name.ToLower());

            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find logged recipe by name: " + model.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                foreach (var cookedRecipeCalledIngredient in cookedRecipe.CookedRecipeCalledIngredients)
                {
                    _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredient.Id);
                }
                _repository.CookedRecipes.Delete(cookedRecipe.Id);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}