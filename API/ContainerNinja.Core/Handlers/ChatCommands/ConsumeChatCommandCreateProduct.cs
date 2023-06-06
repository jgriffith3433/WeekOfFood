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
    public class ConsumeChatCommandCreateProduct : IRequest<ChatResponseVM>
    {
        public ChatAICommandCreateProduct Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandCreateProductHandler : IRequestHandler<ConsumeChatCommandCreateProduct, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandCreateProductHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandCreateProductHandler(ILogger<ConsumeChatCommandCreateProductHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandCreateProduct request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var productEntity = new Product
            {
                Name = request.Command.Product
            };

            //always ensure a product stock record exists for each product
            var productStockEntity = new ProductStock
            {
                Name = request.Command.Product,
                Units = 1
            };
            productStockEntity.Product = productEntity;
            _repository.ProductStocks.Add(productStockEntity);
            return chatResponseVM;
        }
    }
}