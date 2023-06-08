using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "delete_cooked_recipe" })]
    public class ConsumeChatCommandDeleteCookedRecipe : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTODeleteCookedRecipe>
    {
        public ChatAICommandDTODeleteCookedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteCookedRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteCookedRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteCookedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandDeleteCookedRecipe model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(r => r.CookedRecipeCalledIngredients)
                .FirstOrDefault(cr => cr.Recipe.Name.ToLower() == model.Command.Name.ToLower());

            if (cookedRecipe == null)
            {
                var systemResponse = "Error: Could not find cooked recipe by name: " + model.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                foreach (var cookedRecipeCalledIngredient in cookedRecipe.CookedRecipeCalledIngredients)
                {
                    _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredient.Id);
                }
                _repository.CookedRecipes.Delete(cookedRecipe.Id);
                model.Response.ChatMessages.Add(new ChatMessageVM
                {
                    Content = "Success",
                    RawContent = "Success",
                    Name = StaticValues.ChatMessageRoles.System,
                    Role = StaticValues.ChatMessageRoles.System,
                });
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}