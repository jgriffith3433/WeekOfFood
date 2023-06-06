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
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using FluentValidation.Results;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandDeleteCookedRecipe : IRequest<ChatResponseVM>
    {
        public ChatAICommandDeleteCookedRecipe Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandDeleteCookedRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteCookedRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandDeleteCookedRecipeHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandDeleteCookedRecipeHandler(ILogger<ConsumeChatCommandDeleteCookedRecipeHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandDeleteCookedRecipe request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(r => r.CookedRecipeCalledIngredients)
                .FirstOrDefault(cr => cr.Recipe.Name.ToLower() == request.Command.Name.ToLower());

            if (cookedRecipe == null)
            {
                var systemResponse = "Error: Could not find cooked recipe by name: " + request.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else
            {

                foreach (var cookedRecipeCalledIngredient in cookedRecipe.CookedRecipeCalledIngredients)
                {
                    _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredient.Id);
                }
                _repository.CookedRecipes.Delete(cookedRecipe.Id);
            }
            return chatResponseVM;
        }
    }
}