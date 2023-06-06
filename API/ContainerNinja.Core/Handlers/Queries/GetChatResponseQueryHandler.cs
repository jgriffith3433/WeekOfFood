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
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatResponseQuery : IRequest<ChatResponseVM>
    {
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string CurrentUrl { get; set; }
        public string SendToRole { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
        public bool NormalChat { get; set; }
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
            ChatResponseVM chatResponseVM;

            try
            {
                if (request.SendToRole == StaticValues.ChatMessageRoles.Assistant)
                {
                    request.CurrentSystemToAssistantChatCalls++;
                    var rawAssistantResponseMessage = await _chatAIService.GetChatResponse(request.ChatMessages, request.CurrentUrl);
                    var startIndex = rawAssistantResponseMessage.IndexOf('{');
                    var endIndex = rawAssistantResponseMessage.LastIndexOf('}');
                    if (startIndex != -1 && endIndex == -1)
                    {
                        request.ChatMessages.Add(new ChatMessageVM
                        {
                            Content = rawAssistantResponseMessage,
                            RawContent = rawAssistantResponseMessage,
                            Role = StaticValues.ChatMessageRoles.Assistant,
                            Name = StaticValues.ChatMessageRoles.Assistant,
                        });
                        //partial json response
                        chatResponseVM = new ChatResponseVM
                        {
                            ChatConversationId = request.ChatConversation.Id,
                            //CreateNewChat = true,
                            ChatMessages = request.ChatMessages
                        };
                        var systemResponse = "I'm sorry, the AI has sent back a partial response.";
                        chatResponseVM.ChatMessages.Add(new ChatMessageVM
                        {
                            Content = systemResponse,
                            RawContent = systemResponse,
                            Role = StaticValues.ChatMessageRoles.System,
                            Name = StaticValues.ChatMessageRoles.System,
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

                        request.ChatMessages.Add(new ChatMessageVM
                        {
                            Content = chatAICommand.Response,
                            RawContent = rawAssistantResponseMessage,
                            Role = StaticValues.ChatMessageRoles.Assistant,
                            Name = StaticValues.ChatMessageRoles.Assistant,
                        });
                        chatResponseVM = await _mediator.Send(new ConsumeChatCommand
                        {
                            ChatConversation = request.ChatConversation,
                            ChatMessages = request.ChatMessages,
                            ChatAICommand = chatAICommand,
                            RawChatAICommand = rawAssistantResponseMessage,
                            CurrentUrl = request.CurrentUrl,
                        });
                    }
                    else
                    {
                        request.ChatMessages.Add(new ChatMessageVM
                        {
                            Content = rawAssistantResponseMessage,
                            RawContent = rawAssistantResponseMessage,
                            Role = StaticValues.ChatMessageRoles.Assistant,
                            Name = StaticValues.ChatMessageRoles.Assistant,
                        });
                        //no json in response. send to user
                        chatResponseVM = new ChatResponseVM
                        {
                            ChatConversationId = request.ChatConversation.Id,
                            ChatMessages = request.ChatMessages,
                        };
                    }
                }
                else if (request.SendToRole == StaticValues.ChatMessageRoles.System)
                {
                    chatResponseVM = new ChatResponseVM
                    {
                        ChatConversationId = request.ChatConversation.Id,
                        //CreateNewChat = true,
                        ChatMessages = request.ChatMessages,
                    };
                    //TODO: Could implement website AI
                    var systemResponse = "Hello, I cannot receive direct messages yet.";
                    chatResponseVM.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = systemResponse,
                        RawContent = systemResponse,
                        Role = StaticValues.ChatMessageRoles.System,
                        Name = StaticValues.ChatMessageRoles.System,
                    });
                }
                else
                {
                    chatResponseVM = new ChatResponseVM
                    {
                        ChatConversationId = request.ChatConversation.Id,
                        //CreateNewChat = true,
                        ChatMessages = request.ChatMessages,
                    };
                    //TODO: Could implement user to user chat?
                    chatResponseVM.ChatMessages.Add(new ChatMessageVM
                    {
                        Content = request.ChatMessages[request.ChatMessages.Count - 1].Content,
                        RawContent = request.ChatMessages[request.ChatMessages.Count - 1].Content,
                        Role = StaticValues.ChatMessageRoles.User,
                        Name = StaticValues.ChatMessageRoles.User,
                    });
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
                    Name = StaticValues.ChatMessageRoles.System,
                    Role = StaticValues.ChatMessageRoles.System,
                });
                if (request.CurrentSystemToAssistantChatCalls < 7)
                {
                    chatResponseVM = await _mediator.Send(new GetChatResponseQuery
                    {
                        ChatMessages = chatResponseVM.ChatMessages,
                        ChatConversation = request.ChatConversation,
                        CurrentUrl = request.CurrentUrl,
                        SendToRole = StaticValues.ChatMessageRoles.Assistant,
                        CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                    });
                }
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
                    Name = StaticValues.ChatMessageRoles.System,
                    Role = StaticValues.ChatMessageRoles.System,
                });
                if (request.CurrentSystemToAssistantChatCalls < 5)
                {
                    chatResponseVM = await _mediator.Send(new GetChatResponseQuery
                    {
                        ChatMessages = chatResponseVM.ChatMessages,
                        ChatConversation = request.ChatConversation,
                        CurrentUrl = request.CurrentUrl,
                        SendToRole = StaticValues.ChatMessageRoles.Assistant,
                        CurrentSystemToAssistantChatCalls = request.CurrentSystemToAssistantChatCalls,
                    });
                }
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