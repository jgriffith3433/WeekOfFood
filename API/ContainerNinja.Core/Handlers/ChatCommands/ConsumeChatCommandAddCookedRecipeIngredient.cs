using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_cooked_recipe_ingredient" })]
    public class ConsumeChatCommandAddCookedRecipeIngredient : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTOAddCookedRecipeIngredient>
    {
        public ChatAICommandDTOAddCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddCookedRecipeIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandAddCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Name.ToLower() == model.Command.Recipe.ToLower());
            if (cookedRecipe == null)
            {
                var systemResponse = "Error: Could not find cooked recipe by name: " + model.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                {
                    Name = model.Command.Name,
                    //CookedRecipe = cookedRecipe,
                    Units = model.Command.Units,
                    UnitType = model.Command.UnitType.UnitTypeFromString()
                };
                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                _repository.CookedRecipes.Update(cookedRecipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}