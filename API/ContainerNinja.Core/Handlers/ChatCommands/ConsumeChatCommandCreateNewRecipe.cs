using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "create_new_recipe" })]
    public class ConsumeChatCommandCreateNewRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateNewRecipe>
    {
        public ChatAICommandDTOCreateNewRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateNewRecipeHandler : IRequestHandler<ConsumeChatCommandCreateNewRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateNewRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateNewRecipe model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.CreateProxy();
            _repository.Recipes.Add(recipeEntity);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            model.Response.NavigateToPage = "recipes";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(recipeObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}