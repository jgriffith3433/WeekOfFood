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
    public class ConsumeChatCommandCreateCookedRecipe : IRequest<ChatResponseVM>
    {
        public ChatAICommandCreateCookedRecipe Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandCreateCookedRecipeHandler : IRequestHandler<ConsumeChatCommandCreateCookedRecipe, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandCreateCookedRecipeHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandCreateCookedRecipeHandler(ILogger<ConsumeChatCommandCreateCookedRecipeHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCreateCookedRecipe request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock)
                .Include(r => r.CookedRecipes)
                .Where(r => r.Name.ToLower() == request.Command.Name.ToLower())
                .SingleOrDefaultAsync(cancellationToken).Result;

            if (recipe == null)
            {
                var systemResponse = "Error: Could not find recipe by name: " + request.Command.Name;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                var cookedRecipe = new CookedRecipe
                {
                    Recipe = recipe
                };
                foreach (var calledIngredient in recipe.CalledIngredients)
                {
                    var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                    {
                        Name = calledIngredient.Name,
                        CookedRecipe = cookedRecipe,
                        CalledIngredient = calledIngredient,
                        ProductStock = calledIngredient.ProductStock,
                        UnitType = calledIngredient.UnitType,
                        Units = calledIngredient.Units != null ? calledIngredient.Units.Value : 0
                    };
                    cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                }
                recipe.CookedRecipes.Add(cookedRecipe);
                _repository.Recipes.Update(recipe);
            }
            return chatResponseVM;
        }
    }
}