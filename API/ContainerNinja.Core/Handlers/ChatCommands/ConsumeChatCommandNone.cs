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
using FluentValidation;
using ContainerNinja.Core.Handlers.Queries;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandNone : IRequest<ChatResponseVM>
    {
        public ChatAICommandNone Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandNoneHandler : IRequestHandler<ConsumeChatCommandNone, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandNoneHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;
        private readonly IValidator<ConsumeChatCommandNone> _validator;

        public ConsumeChatCommandNoneHandler(ILogger<ConsumeChatCommandNoneHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator, IValidator<ConsumeChatCommandNone> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
            _validator = validator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandNone request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
                ChatConversationId = request.ChatConversation.Id,
                UnknownCommand = !string.IsNullOrEmpty(request.Command.Cmd)
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
                //Sometimes the AI sends responses back with "none" as the command while communicating
            }

            return chatResponseVM;
        }
    }
}