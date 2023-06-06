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
using ContainerNinja.Core.Exceptions;
using FluentValidation.Results;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandEditProductUnitType : IRequest<ChatResponseVM>
    {
        public ChatAICommandEditProductUnitType Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandEditProductUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditProductUnitType, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandEditProductUnitTypeHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandEditProductUnitTypeHandler(ILogger<ConsumeChatCommandEditProductUnitTypeHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandEditProductUnitType request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var product = _repository.Products.FirstOrDefault(p => p.Name.ToLower().Contains(request.Command.Product.ToLower()));
            if (product == null)
            {
                var systemResponse = "Error: Could not find product by name: " + request.Command.Product;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                product.UnitType = request.Command.UnitType.UnitTypeFromString();
                _repository.Products.Update(product);
            }
            return chatResponseVM;
        }
    }
}