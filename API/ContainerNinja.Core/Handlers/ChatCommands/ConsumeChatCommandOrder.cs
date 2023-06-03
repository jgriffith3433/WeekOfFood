using MediatR;
using ContainerNinja.Contracts.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ChatAI;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Handlers.Queries;
using FluentValidation;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandOrder : IRequest<ChatResponseVM>
    {
        public ChatAICommandOrder Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandOrderHandler : IRequestHandler<ConsumeChatCommandOrder, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandOrderHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;
        private readonly IValidator<ConsumeChatCommandOrder> _validator;

        public ConsumeChatCommandOrderHandler(ILogger<ConsumeChatCommandOrderHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator, IValidator<ConsumeChatCommandOrder> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
            _validator = validator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandOrder request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatConversationId = request.ChatConversation.Id,
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
                var systemResponse = "Error: Not implemented: " + request.Command.Cmd;
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = systemResponse,
                    RawContent = systemResponse,
                    Role = StaticValues.ChatMessageRoles.System,
                    Name = StaticValues.ChatMessageRoles.System,
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
            return chatResponseVM;
        }
    }
}