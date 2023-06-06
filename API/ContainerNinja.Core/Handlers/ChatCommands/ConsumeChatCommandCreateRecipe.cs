using MediatR;
using ContainerNinja.Contracts.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ChatAI;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Core.Handlers.Queries;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandCreateRecipe : IRequest<ChatResponseVM>
    {
        public ChatAICommandCreateRecipe Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandCreateRecipeHandler : IRequestHandler<ConsumeChatCommandCreateRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandCreateRecipeHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandCreateRecipeHandler(ILogger<ConsumeChatCommandCreateRecipeHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCreateRecipe request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            //var response = _mediator.Send(new CreateRecipeCommand
            //{
            //    Name = createRecipe.Name,
            //    UserImport = notification.ChatCommand.RawReponse
            //});
            var recipeEntity = new Recipe();

            recipeEntity.Name = request.Command.Name;
            //entity.Serves = createRecipe.Serves.Value;
            recipeEntity.UserImport = request.RawChatAICommand;

            foreach (var createRecipeIngredient in request.Command.Ingredients)
            {
                var calledIngredient = new CalledIngredient
                {
                    Name = createRecipeIngredient.Name,
                    Recipe = recipeEntity,
                    Verified = false,
                    Units = createRecipeIngredient.Units,
                    UnitType = createRecipeIngredient.UnitType.UnitTypeFromString()
                };
                recipeEntity.CalledIngredients.Add(calledIngredient);
            }

            _repository.Recipes.Add(recipeEntity);
            //await _repository.CommitAsync();

            //if (entity.UserImport != null)
            //{
            //    entity.AddDomainEvent(new RecipeUserImportEvent(entity));
            //}
            //await _repository.SaveChangesAsync(cancellationToken);
            return chatResponseVM;
        }
    }
}