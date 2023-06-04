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
    public class ConsumeChatCommandAddRecipeIngredient : IRequest<ChatResponseVM>
    {
        public ChatAICommandAddRecipeIngredient Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandAddRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandAddRecipeIngredient, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandAddRecipeIngredientHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;
        private readonly IValidator<ConsumeChatCommandAddRecipeIngredient> _validator;

        public ConsumeChatCommandAddRecipeIngredientHandler(ILogger<ConsumeChatCommandAddRecipeIngredientHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator, IValidator<ConsumeChatCommandAddRecipeIngredient> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
            _validator = validator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandAddRecipeIngredient request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var result = _validator.Validate(request);

            _logger.LogInformation($"Validation result: {result}");

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    chatResponseVM.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = error.ErrorMessage,
                        RawContent = error.ErrorMessage,
                        Name = StaticValues.ChatMessageRoles.System,
                        Role = StaticValues.ChatMessageRoles.System,
                    });
                }
                chatResponseVM = await _mediator.Send(new GetChatResponseQuery
                {
                    ChatMessages = chatResponseVM.ChatMessages,
                    ChatConversation = request.ChatConversation,
                    CurrentUrl = request.CurrentUrl,
                    SendToRole = StaticValues.ChatMessageRoles.Assistant,
                    CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                });
            }
            else
            {
                //Command logic
                var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == request.Command.Recipe.ToLower());
                if (recipe == null)
                {
                    var systemResponse = "Error: Could not find recipe by name: " + request.Command.Recipe;
                    chatResponseVM.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = systemResponse,
                        RawContent = systemResponse,
                        Name = StaticValues.ChatMessageRoles.System,
                        Role = StaticValues.ChatMessageRoles.System,
                    });
                    chatResponseVM = await _mediator.Send(new GetChatResponseQuery
                    {
                        ChatMessages = chatResponseVM.ChatMessages,
                        ChatConversation = request.ChatConversation,
                        CurrentUrl = request.CurrentUrl,
                        SendToRole = StaticValues.ChatMessageRoles.Assistant,
                        CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                    });
                }
                else
                {
                    var calledIngredient = new CalledIngredient
                    {
                        Name = request.Command.Name,
                        Recipe = recipe,
                        Verified = false,
                        Units = request.Command.Units,
                        UnitType = request.Command.UnitType.UnitTypeFromString()
                    };
                    recipe.CalledIngredients.Add(calledIngredient);
                    _repository.Recipes.Update(recipe);
                }
            }
            return chatResponseVM;
        }
    }
}