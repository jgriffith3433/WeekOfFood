using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.DTO.ChatAICommands;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_logged_recipe_ingredient" })]
    public class ConsumeChatCommandAddCookedRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddCookedRecipeIngredient>
    {
        public ChatAICommandDTOAddCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddCookedRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Name.ToLower() == model.Command.RecipeName.ToLower());
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find logged recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = _repository.CookedRecipeCalledIngredients.CreateProxy();
                {
                    cookedRecipeCalledIngredient.Name = model.Command.IngredientName;
                    //cookedRecipeCalledIngredient.CookedRecipe = cookedRecipe;
                    cookedRecipeCalledIngredient.Units = model.Command.Units;
                    cookedRecipeCalledIngredient.UnitType = model.Command.UnitType.UnitTypeFromString();
                };
                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                _repository.CookedRecipes.Update(cookedRecipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Successfully logged recipe: " + model.Command.RecipeName;
        }
    }
}