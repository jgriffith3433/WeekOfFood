using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "edit_recipe_name" })]
    public class ConsumeChatCommandEditRecipeName : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditRecipeName>
    {
        public ChatAICommandDTOEditRecipeName Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditRecipeNameHandler : IRequestHandler<ConsumeChatCommandEditRecipeName, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditRecipeNameHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditRecipeName model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.Id);

            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by id '" + model.Command.Id;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_recipe_id"" }");
            }

            var existingRecipeWithName = _repository.Recipes.Set.FirstOrDefault(r => r.Name.ToLower() == model.Command.NewName.ToLower());
            if (existingRecipeWithName != null)
            {
                var systemResponse = "Recipe already exists: " + model.Command.NewName;
                throw new ChatAIException(systemResponse);
            }

            recipe.Name = model.Command.NewName;
            _repository.Recipes.Update(recipe);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}