using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_recipe_ingredient" })]
    public class ConsumeChatCommandDeleteRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteRecipeIngredient>
    {
        public ChatAICommandDTODeleteRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandDeleteRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Set.FirstOrDefault(r => r.Name.ToLower() == model.Command.RecipeName.ToLower());
            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }

            var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.IngredientName.ToLower()));

            if (calledIngredient == null)
            {
                var systemResponse = "No ingredient '" + model.Command.IngredientName + "' found on recipe '" + model.Command.RecipeName + "'. The ingredients are: " + string.Join(", ", recipe.CalledIngredients.Select(ci => ci.Name));
                throw new ChatAIException(systemResponse);
            }

            recipe.CalledIngredients.Remove(calledIngredient);
            _repository.Recipes.Update(recipe);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}