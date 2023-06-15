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
    [ChatCommandModel(new [] { "add_recipe_log_ingredient" })]
    [ChatCommandSpecification("add_recipe_log_ingredient", "Add an ingredient to a logged recipe.",
@"{
    ""type"": ""object"",
    ""properties"": {
        ""recipename"": {
            ""type"": ""string"",
            ""description"": ""The name of the logged recipe.""
        },
        ""ingredientname"": {
            ""type"": ""string"",
            ""description"": ""The name of the ingredient to add.""
        },
        ""units"": {
            ""type"": ""number"",
            ""description"": ""How many units of the ingredient.""
        },
        ""unittype"": {
            ""type"": ""string"",
            ""enum"": [""none"", ""bulk"", ""ounce"", ""teaspoon"", ""tablespoon"", ""pound"", ""cup"", ""clove"", ""can"", ""whole"", ""package"", ""bar"", ""bun"", ""bottle""],
            ""description"": ""The unit type of the ingredient.""
        }
    },
    ""required"": [""recipename"", ""ingredientname"", ""units"", ""unittype""]
}")]
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
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Name.ToLower() == model.Command.RecipeName.ToLower());
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find logged recipe by name: " + model.Command.RecipeName;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                {
                    Name = model.Command.IngredientName,
                    //CookedRecipe = cookedRecipe,
                    Units = model.Command.Units,
                    UnitType = model.Command.UnitType.UnitTypeFromString()
                };
                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                _repository.CookedRecipes.Update(cookedRecipe);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Successfully logged recipe: " + model.Command.RecipeName;
        }
    }
}