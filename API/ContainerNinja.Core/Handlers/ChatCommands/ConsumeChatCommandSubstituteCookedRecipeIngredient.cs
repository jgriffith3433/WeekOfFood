using MediatR;
using ContainerNinja.Contracts.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "substitute_cooked_recipe_ingredient" })]
    public class ConsumeChatCommandSubstituteCookedRecipeIngredient : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOSubstituteCookedRecipeIngredient>
    {
        public ChatAICommandDTOSubstituteCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandSubstituteCookedRecipeIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandSubstituteCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.CalledIngredient).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.ProductStock).OrderByDescending(cr => cr.Created).FirstOrDefault(cr => cr.Recipe.Name.ToLower().Contains(model.Command.Recipe.ToLower()));
            if (cookedRecipe == null)
            {
                var systemResponse = "Error: Could not find cooked recipe by name: " + model.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.Original.ToLower()));

                if (cookedRecipeCalledIngredient == null)
                {
                    var systemResponse = "Error: Could not find ingredient by name: " + model.Command.Original;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    cookedRecipeCalledIngredient.Name = model.Command.New;
                    cookedRecipeCalledIngredient.CalledIngredient = null;
                    cookedRecipeCalledIngredient.ProductStock = null;
                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                }
            }
            return model.Response;
        }
    }
}