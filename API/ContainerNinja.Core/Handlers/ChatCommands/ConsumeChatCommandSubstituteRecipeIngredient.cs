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
    [ChatCommandModel(new [] { "substitute_recipe_ingredient" })]
    public class ConsumeChatCommandSubstituteRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSubstituteRecipeIngredient>
    {
        public ChatAICommandDTOSubstituteRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandSubstituteRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSubstituteRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Set.FirstOrDefault(r => r.Name.ToLower() == model.Command.RecipeName.ToLower());
            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.OriginalIngredient.ToLower()));

                if (calledIngredient == null)
                {
                    var systemResponse = "Could not find ingredient by name: " + model.Command.OriginalIngredient;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    calledIngredient.Name = model.Command.NewIngredient;
                    calledIngredient.ProductStock = null;
                    _repository.CalledIngredients.Update(calledIngredient);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}