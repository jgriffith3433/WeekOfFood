using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using System.Text;
using ContainerNinja.Contracts.ChatAI;
using OpenAI.ObjectModels;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetNormalChatResponseQuery : IRequest<ChatResponseVM>
    {
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string CurrentUrl { get; set; }
        public string SendToRole { get; set; }
    }

    public class GetNormalChatResponseQueryHandler : IRequestHandler<GetNormalChatResponseQuery, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public GetNormalChatResponseQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator, IValidator<GetChatResponseQuery> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(GetNormalChatResponseQuery request, CancellationToken cancellationToken)
        {
            ChatResponseVM chatResponseVM;

            try
            {
                var rawAssistantResponseMessage = await _chatAIService.GetNormalChatResponse(request.ChatMessages);

                request.ChatMessages.Add(new ChatMessageVM
                {
                    Content = rawAssistantResponseMessage,
                    RawContent = rawAssistantResponseMessage,
                    Role = StaticValues.ChatMessageRoles.Assistant,
                    Name = StaticValues.ChatMessageRoles.Assistant,
                });
                chatResponseVM = new ChatResponseVM
                {
                    ChatConversationId = request.ChatConversation.Id,
                    ChatMessages = request.ChatMessages,
                };
            }
            catch (Exception e)
            {
                chatResponseVM = new ChatResponseVM
                {
                    ChatConversationId = request.ChatConversation.Id,
                    //CreateNewChat = true,
                    ChatMessages = request.ChatMessages,
                    Error = true
                };
                request.ChatConversation.Error = FlattenException(e);
            }
            _cache.Clear();

            request.ChatConversation.Content = JsonConvert.SerializeObject(request);
            _repository.ChatConversations.Update(request.ChatConversation);
            await _repository.CommitAsync();

            return chatResponseVM;
        }

        private string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }
    }
}