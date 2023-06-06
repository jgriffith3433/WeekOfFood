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
using LinqKit;
using ContainerNinja.Core.Exceptions;
using FluentValidation.Results;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommandDeleteProduct : IRequest<ChatResponseVM>
    {
        public ChatAICommandDeleteProduct Command { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandDeleteProductHandler : IRequestHandler<ConsumeChatCommandDeleteProduct, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandDeleteProductHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandDeleteProductHandler(ILogger<ConsumeChatCommandDeleteProductHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandDeleteProduct request, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatMessages = request.ChatMessages,
            };
            var predicate = PredicateBuilder.New<Product>();
            var searchTerms = string.Join(' ', request.Command.Product.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }
            var query = _repository.Products.Include<Product, ProductStock>(p => p.ProductStock)
                .AsNoTracking()
                .AsExpandable()
                .Where(predicate).ToList();

            if (query.Count == 0)
            {
                var systemResponse = "Error: Could not find product by name: " + request.Command.Product;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                if (query.Count == 1 && query[0].Name.ToLower() == request.Command.Product.ToLower())
                {
                    //exact match, go ahead and delete
                    _repository.Products.Delete(query[0].Id);
                }
                else
                {
                    //unsure, ask user
                    var productNames = query.Select(p => p.Name).ToList();
                    var systemResponse = "Which product are you referring to?\n" + string.Join(", ", productNames);
                    throw new ChatAIException(systemResponse);
                }
            }
            return chatResponseVM;
        }
    }
}