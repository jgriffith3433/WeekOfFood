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

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandTemplate : IRequest<ChatResponseVM>
    {
        public ChatAICommandTemplate Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandTemplateHandler : IRequestHandler<ConsumeChatCommandTemplate, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandTemplateHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandTemplateHandler(ILogger<ConsumeChatCommandTemplateHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandTemplate request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            //Command logic

            return chatResponseVM;
        }
    }
}