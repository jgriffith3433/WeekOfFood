using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_recipe" })]
    public class ConsumeChatCommandDeleteRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteRecipe>
    {
        public ChatAICommandDTODeleteRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteRecipe model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""search_recipes"" }");
            }
            _repository.Recipes.Delete(recipeEntity.Id);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            recipeObject["RecipeName"] = recipeEntity.Name;
            model.Response.NavigateToPage = "recipes";
            return "Deleted recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}