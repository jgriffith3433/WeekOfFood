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
    [ChatCommandModel(new [] { "substitute_logged_recipe_ingredient" })]
    public class ConsumeChatCommandSubstituteCookedRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSubstituteCookedRecipeIngredient>
    {
        public ChatAICommandDTOSubstituteCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandSubstituteCookedRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSubstituteCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(cr => cr.Recipe.Name.ToLower().Contains(model.Command.RecipeName.ToLower()));
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find cooked recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.OriginalIngredient.ToLower()));

                if (cookedRecipeCalledIngredient == null)
                {
                    var systemResponse = "Could not find ingredient by name: " + model.Command.OriginalIngredient;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    cookedRecipeCalledIngredient.Name = model.Command.NewIngredient;
                    cookedRecipeCalledIngredient.CalledIngredient = null;
                    cookedRecipeCalledIngredient.ProductStock = null;
                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}