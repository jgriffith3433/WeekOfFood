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
    [ChatCommandModel(new[] { "create_new_kitchen_inventory" })]
    public class ConsumeChatCommandCreateNewKitchenInventory : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateNewKitchenInventory>
    {
        public ChatAICommandDTOCreateNewKitchenInventory Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateNewKitchenInventoryHandler : IRequestHandler<ConsumeChatCommandCreateNewKitchenInventory, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateNewKitchenInventoryHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateNewKitchenInventory model, CancellationToken cancellationToken)
        {
            //var recipeEntity = _repository.Recipes.CreateProxy();
            //_repository.Recipes.Add(recipeEntity);

            //model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //await _repository.CommitAsync();

            var kitchenInventoryObject = new JObject();
            //kitchenInventoryObject["KitchenInventoryId"] = recipeEntity.Id;
            //kitchenInventoryObject["KitchenInventoryId"] = 1;
            model.Response.NavigateToPage = "kitchen-products";
            //model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(kitchenInventoryObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}