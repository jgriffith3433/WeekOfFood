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
    public class ConsumeChatCommandEditRecipeIngredientUnitType : IRequest<ChatResponseVM>
    {
        public ChatAICommandEditRecipeIngredientUnitType Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandEditRecipeIngredientUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditRecipeIngredientUnitType, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandEditRecipeIngredientUnitTypeHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;
        private readonly IValidator<ConsumeChatCommandEditRecipeIngredientUnitType> _validator;

        public ConsumeChatCommandEditRecipeIngredientUnitTypeHandler(ILogger<ConsumeChatCommandEditRecipeIngredientUnitTypeHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator, IValidator<ConsumeChatCommandEditRecipeIngredientUnitType> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
            _validator = validator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandEditRecipeIngredientUnitType request, CancellationToken cancellationToken)
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
                var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower().Contains(request.Command.Recipe.ToLower()));

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
                    var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(request.Command.Name.ToLower()));
                    if (calledIngredient == null)
                    {
                        var systemResponse = "Error: Could not find ingredient by name: " + request.Command.Name;
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
                        calledIngredient.UnitType = request.Command.UnitType.UnitTypeFromString();
                        _repository.CalledIngredients.Update(calledIngredient);
                    }
                }
            }
            return chatResponseVM;
        }
    }
}