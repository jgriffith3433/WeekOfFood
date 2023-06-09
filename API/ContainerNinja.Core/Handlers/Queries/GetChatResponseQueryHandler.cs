using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using System.Text;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using OpenAI.ObjectModels;
using ContainerNinja.Core.Handlers.ChatCommands;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatResponseQuery : IRequest<ChatResponseVM>
    {
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string CurrentUrl { get; set; }
        public bool NormalChat { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
        public string NavigateToPage { get; set; }
        public bool Dirty { get; set; }
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

        public async Task<ChatResponseVM> Handle(GetChatResponseQuery request, CancellationToken cancellationToken)
        {
            foreach (var chatMessage in request.ChatMessages)
            {
                if (chatMessage.To == StaticValues.ChatMessageRoles.System)
                {
                    chatMessage.Received = true;
                }
            }
            ChatResponseVM chatResponseVM;

            try
            {
                request.CurrentSystemToAssistantChatCalls++;
                var rawAssistantResponseMessage = await _chatAIService.GetChatResponse(request.ChatMessages, request.CurrentUrl);
                foreach (var chatMessage in request.ChatMessages)
                {
                    if (chatMessage.To == StaticValues.ChatMessageRoles.Assistant)
                    {
                        chatMessage.Received = true;
                    }
                }
                var startIndex = rawAssistantResponseMessage.IndexOf('{');
                var endIndex = rawAssistantResponseMessage.LastIndexOf('}');
                if (startIndex != -1 && endIndex == -1)
                {
                    //partial json response

                    //Since there was a curly bracket in the response
                    //Assume the message was for the system
                    request.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = rawAssistantResponseMessage,
                        RawContent = rawAssistantResponseMessage,
                        From = StaticValues.ChatMessageRoles.Assistant,
                        To = StaticValues.ChatMessageRoles.System,
                    });
                    chatResponseVM = new ChatResponseVM
                    {
                        ChatConversationId = request.ChatConversation.Id,
                        ChatMessages = request.ChatMessages,
                        Error = true,
                        UnknownCommand = true,
                    };
                    var systemResponse = "I'm sorry, the AI has sent back a partial response.";
                    chatResponseVM.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = systemResponse,
                        RawContent = systemResponse,
                        From = StaticValues.ChatMessageRoles.System,
                        To = StaticValues.ChatMessageRoles.User,
                    });
                }
                else if (startIndex != -1 && endIndex != -1)
                {
                    //json response
                    var chatAICommand = JsonConvert.DeserializeObject<ChatAICommandDTO>(rawAssistantResponseMessage.Substring(startIndex, endIndex - startIndex + 1));

                    //sometimes the ai sends the response before the json brackets
                    if (string.IsNullOrEmpty(chatAICommand.Response))
                    {
                        //to account for newlines, skip the first 2 characters when deciding if the response is outside
                        if (startIndex > 2)
                        {
                            chatAICommand.Response = rawAssistantResponseMessage.Substring(0, startIndex + 1);
                        }
                    }

                    //by default all json responses go to the system
                    //the system will then determine if the chat command is 'None'
                    //then the response will go to the user
                    request.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = chatAICommand.Response,
                        RawContent = rawAssistantResponseMessage,
                        From = StaticValues.ChatMessageRoles.Assistant,
                        To = StaticValues.ChatMessageRoles.System,
                    });
                    chatResponseVM = await _mediator.Send(new ConsumeChatCommand
                    {
                        ChatConversation = request.ChatConversation,
                        ChatMessages = request.ChatMessages,
                        ChatAICommand = chatAICommand,
                        RawChatAICommand = rawAssistantResponseMessage,
                        CurrentUrl = request.CurrentUrl,
                        CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                        NavigateToPage = request.NavigateToPage,
                        Dirty = request.Dirty,
                    });
                }
                else
                {
                    //no json in response. send to user
                    request.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = rawAssistantResponseMessage,
                        RawContent = rawAssistantResponseMessage,
                        From = StaticValues.ChatMessageRoles.Assistant,
                        To = StaticValues.ChatMessageRoles.User,
                    });
                    chatResponseVM = new ChatResponseVM
                    {
                        ChatConversationId = request.ChatConversation.Id,
                        ChatMessages = request.ChatMessages,
                    };
                }
            }
            catch (ApiValidationException ex)
            {
                chatResponseVM = new ChatResponseVM
                {
                    ChatMessages = request.ChatMessages,
                };
                var systemResponse = ex.Message + "\n";
                foreach (var e in ex.Errors)
                {
                    foreach (var v in e.Value)
                    {
                        systemResponse += v + "\n";
                    }
                }
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = systemResponse,
                    RawContent = systemResponse,
                    From = StaticValues.ChatMessageRoles.System,
                    To = StaticValues.ChatMessageRoles.Assistant,
                });
                chatResponseVM = await _mediator.Send(new GetChatResponseQuery
                {
                    ChatMessages = chatResponseVM.ChatMessages,
                    ChatConversation = request.ChatConversation,
                    CurrentUrl = request.CurrentUrl,
                    CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                    NavigateToPage = request.NavigateToPage,
                    Dirty = chatResponseVM.Dirty,
                });
                chatResponseVM.ChatConversationId = request.ChatConversation.Id;
            }
            catch (ChatAIException ex)
            {
                chatResponseVM = new ChatResponseVM
                {
                    ChatMessages = request.ChatMessages,
                };
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = ex.Message,
                    RawContent = ex.Message,
                    From = StaticValues.ChatMessageRoles.System,
                    To = StaticValues.ChatMessageRoles.Assistant,
                });
                chatResponseVM = await _mediator.Send(new GetChatResponseQuery
                {
                    ChatMessages = chatResponseVM.ChatMessages,
                    ChatConversation = request.ChatConversation,
                    CurrentUrl = request.CurrentUrl,
                    CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                    NavigateToPage = request.NavigateToPage,
                    Dirty = chatResponseVM.Dirty,
                });
                chatResponseVM.ChatConversationId = request.ChatConversation.Id;
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