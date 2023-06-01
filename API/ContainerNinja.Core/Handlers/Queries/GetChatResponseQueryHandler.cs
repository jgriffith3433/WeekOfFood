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

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatResponseQuery : IRequest<GetChatResponseVM>
    {
        public List<ChatMessageVM> PreviousMessages { get; set; } = new List<ChatMessageVM> { };
        public ChatMessageVM ChatMessage { get; set; }
        public int? ChatConversationId { get; set; }
        public string CurrentUrl { get; set; }
    }

    public class GetChatResponseQueryHandler : IRequestHandler<GetChatResponseQuery, GetChatResponseVM>
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

        public async Task<GetChatResponseVM> Handle(GetChatResponseQuery request, CancellationToken cancellationToken)
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
            var dirty = false;
            var error = false;
            var navigateToPage = "";
            var chatResponseMessage = "";
            try
            {
                //try a few times without user intervention to get a command from openai
                var numTries = 3;
                //1-Assistant, 2-User, 3-System
                var currentChatFrom = request.ChatMessage.From;
                var currentChatMessage = request.ChatMessage.Message;
                for (var i = 0; i < numTries; i++)
                {
                    chatResponseMessage = await _chatAIService.GetChatResponse(currentChatMessage, currentChatFrom, request.PreviousMessages, request.CurrentUrl);
                    request.PreviousMessages.Add(new ChatMessageVM
                    {
                        Message = currentChatMessage,
                        From = currentChatFrom
                    });
                    request.PreviousMessages.Add(new ChatMessageVM
                    {
                        Message = chatResponseMessage,
                        From = 1,
                    });

                    var chatCommandEntity = new ChatCommand
                    {
                        RawReponse = chatResponseMessage,
                        CurrentUrl = request.CurrentUrl
                    };
                    var startIndex = chatResponseMessage.IndexOf('{');
                    var endIndex = chatResponseMessage.LastIndexOf('}');
                    if (startIndex != -1 && endIndex == -1)
                    {
                        //partial json response
                        chatResponseMessage = "I'm sorry, I'm having trouble contacting the server.";
                        error = true;
                        break;
                    }
                    else if (startIndex != -1 && endIndex != -1)
                    {
                        //json response
                        chatResponseMessage = chatResponseMessage.Substring(startIndex, endIndex - startIndex + 1);
                        var openApiChatCommand = JsonConvert.DeserializeObject<ChatAICommand>(chatResponseMessage);

                        chatCommandEntity.CommandName = openApiChatCommand.Cmd;
                        chatCommandEntity.ChatConversation = chatConversationEntity;
                        _repository.ChatCommands.Add(chatCommandEntity);

                        await _repository.CommitAsync();
                        await _mediator.Send(new ReceivedChatCommand
                        {
                            ChatCommand = chatCommandEntity
                        });

                        dirty = chatCommandEntity.ChangedData;
                        navigateToPage = chatCommandEntity.NavigateToPage;

                        if (string.IsNullOrEmpty(chatCommandEntity.SystemResponse))
                        {
                            //success
                            chatResponseMessage = openApiChatCommand.Response;
                            break;
                        }
                        else
                        {
                            //error in handling command, system communicating with openai
                            currentChatMessage = chatCommandEntity.SystemResponse;
                            currentChatFrom = 3;
                        }
                    }
                    else
                    {
                        //no json in response. send to user
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                error = true;
                chatConversationEntity.Error = FlattenException(e);
                await _repository.CommitAsync();
            }
            _cache.Clear();

            chatConversationEntity.Content = JsonConvert.SerializeObject(request);
            _repository.ChatConversations.Update(chatConversationEntity);
            await _repository.CommitAsync();

            return new GetChatResponseVM
            {
                ChatConversationId = chatConversationEntity.Id,
                Dirty = dirty,
                Error = error,
                CreateNewChat = error || !string.IsNullOrEmpty(navigateToPage),
                NavigateToPage = navigateToPage,
                ResponseMessage = new ChatMessageVM
                {
                    From = 1,
                    Message = chatResponseMessage
                },
                PreviousMessages = request.PreviousMessages
            };
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