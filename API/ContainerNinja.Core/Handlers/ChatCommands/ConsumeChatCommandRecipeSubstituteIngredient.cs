using MediatR;
using ContainerNinja.Contracts.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ChatAI;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandRecipeSubstituteIngredient : IRequest<ChatResponseVM>
    {
        public ChatAICommandRecipeSubstituteIngredient Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandRecipeSubstituteIngredientHandler : IRequestHandler<ConsumeChatCommandRecipeSubstituteIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandRecipeSubstituteIngredientHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandRecipeSubstituteIngredientHandler(ILogger<ConsumeChatCommandRecipeSubstituteIngredientHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandRecipeSubstituteIngredient request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock).FirstOrDefault(r => r.Name.ToLower() == request.Command.Recipe.ToLower());
            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + request.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(request.Command.Original.ToLower()));

                if (calledIngredient == null)
                {
                    var systemResponse = "Error: Could not find ingredient by name: " + request.Command.Original;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    calledIngredient.Name = request.Command.New;
                    calledIngredient.ProductStock = null;
                    _repository.CalledIngredients.Update(calledIngredient);
                }
            }
            return chatResponseVM;
        }
    }
}