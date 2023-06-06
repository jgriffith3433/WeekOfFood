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
    public class ConsumeChatCommandCookedRecipeSubstituteIngredient : IRequest<ChatResponseVM>
    {
        public ChatAICommandCookedRecipeSubstituteIngredient Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandCookedRecipeSubstituteIngredientHandler : IRequestHandler<ConsumeChatCommandCookedRecipeSubstituteIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandCookedRecipeSubstituteIngredientHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandCookedRecipeSubstituteIngredientHandler(ILogger<ConsumeChatCommandCookedRecipeSubstituteIngredientHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCookedRecipeSubstituteIngredient request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.CalledIngredient).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.ProductStock).OrderByDescending(cr => cr.Created).FirstOrDefault(cr => cr.Recipe.Name.ToLower().Contains(request.Command.Recipe.ToLower()));
            if (cookedRecipe == null)
            {
                var systemResponse = "Error: Could not find cooked recipe by name: " + request.Command.Recipe;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(request.Command.Original.ToLower()));

                if (cookedRecipeCalledIngredient == null)
                {
                    var systemResponse = "Error: Could not find ingredient by name: " + request.Command.Original;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    cookedRecipeCalledIngredient.Name = request.Command.New;
                    cookedRecipeCalledIngredient.CalledIngredient = null;
                    cookedRecipeCalledIngredient.ProductStock = null;
                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                }
            }
            return chatResponseVM;
        }
    }
}