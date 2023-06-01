using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using System.Text;
using ContainerNinja.Contracts.ChatAI;
using ContainerNinja.Core.Handlers.Commands;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.ResponseModels;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatResponseQuery : IRequest<ChatResponseVM>
    {
        public List<ChatMessage> ChatMessages { get; set; }
        public int? ChatConversationId { get; set; }
        public string CurrentUrl { get; set; }
        public string SendToRole { get; set; }
    }

    public class GetChatResponseQueryHandler : IRequestHandler<GetChatResponseQuery, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public GetChatResponseQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        private async Task<ChatConversation> GetChatConversationEntity(GetChatResponseQuery request)
        {
            ChatConversation chatConversationEntity;
            if (request.ChatConversationId.HasValue && request.ChatConversationId.Value != -1)
            {
                chatConversationEntity = _repository.ChatConversations.FirstOrDefault(cc => cc.Id == request.ChatConversationId.Value);
                if (chatConversationEntity == null)
                {
                    throw new Exception("Not Found");
                }
                chatConversationEntity.Content = JsonConvert.SerializeObject(request);
                _repository.ChatConversations.Update(chatConversationEntity);
            }
            else
            {
                chatConversationEntity = new ChatConversation
                {
                    Content = JsonConvert.SerializeObject(request),
                };
                _repository.ChatConversations.Add(chatConversationEntity);
            }

            await _repository.CommitAsync();
            return chatConversationEntity;
        }

        public async Task<ChatResponseVM> Handle(GetChatResponseQuery request, CancellationToken cancellationToken)
        {
            var chatConversationEntity = await GetChatConversationEntity(request);

            ChatResponseVM chatResponseVM;

            try
            {
                if (request.SendToRole == StaticValues.ChatMessageRoles.Assistant)
                {
                    var assistantResponseMessage = await _chatAIService.GetChatResponse(request.ChatMessages, request.CurrentUrl);
                    var startIndex = assistantResponseMessage.IndexOf('{');
                    var endIndex = assistantResponseMessage.LastIndexOf('}');
                    if (startIndex != -1 && endIndex == -1)
                    {
                        //partial json response
                        var messageToUser = "I'm sorry, the AI has sent back a partial response.";
                        chatResponseVM = new ChatResponseVM
                        {
                            ChatConversationId = chatConversationEntity.Id,
                            CreateNewChat = true,
                            ChatMessages = request.ChatMessages
                        };
                        chatResponseVM.ChatMessages.Add(ChatMessage.FromAssistant(assistantResponseMessage, StaticValues.ChatMessageRoles.Assistant));
                        chatResponseVM.ChatMessages.Add(ChatMessage.FromSystem(messageToUser, StaticValues.ChatMessageRoles.System));
                        chatResponseVM.Error = true;
                    }
                    else if (startIndex != -1 && endIndex != -1)
                    {
                        //json response
                        var chatAICommand = JsonConvert.DeserializeObject<ChatAICommand>(assistantResponseMessage.Substring(startIndex, endIndex - startIndex + 1));

                        chatResponseVM = await _mediator.Send(new ConsumeChatCommand
                        {
                            ChatConversation = chatConversationEntity,
                            ChatMessages = request.ChatMessages,
                            ChatAICommand = chatAICommand,
                            RawChatAICommand = assistantResponseMessage,
                        });
                    }
                    else
                    {
                        //no json in response. send to user
                        chatResponseVM = new ChatResponseVM
                        {
                            ChatConversationId = chatConversationEntity.Id,
                            CreateNewChat = true,
                            ChatMessages = request.ChatMessages,
                        };
                        chatResponseVM.ChatMessages.Add(ChatMessage.FromAssistant(assistantResponseMessage, StaticValues.ChatMessageRoles.Assistant));
                    }
                }
                else if (request.SendToRole == StaticValues.ChatMessageRoles.System)
                {
                    chatResponseVM = new ChatResponseVM
                    {
                        ChatConversationId = chatConversationEntity.Id,
                        CreateNewChat = true,
                        ChatMessages = request.ChatMessages,
                    };
                    //TODO: Could implement website AI
                    chatResponseVM.ChatMessages.Add(ChatMessage.FromSystem("Hello, I cannot receive direct messages yet.", StaticValues.ChatMessageRoles.System));
                }
                else
                {
                    chatResponseVM = new ChatResponseVM
                    {
                        ChatConversationId = chatConversationEntity.Id,
                        CreateNewChat = true,
                        ChatMessages = request.ChatMessages,
                    };
                    //TODO: Could implement user to user chat?
                    chatResponseVM.ChatMessages.Add(ChatMessage.FromUser(request.ChatMessages[request.ChatMessages.Count - 1].Content, StaticValues.ChatMessageRoles.User));
                }
            }
            catch (Exception e)
            {
                chatResponseVM = new ChatResponseVM
                {
                    ChatConversationId = chatConversationEntity.Id,
                    CreateNewChat = true,
                    ChatMessages = request.ChatMessages,
                    Error = true
                };
                chatConversationEntity.Error = FlattenException(e);
            }
            _cache.Clear();

            chatConversationEntity.Content = JsonConvert.SerializeObject(request);
            _repository.ChatConversations.Update(chatConversationEntity);
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