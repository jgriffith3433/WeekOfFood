using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using System.Text;
using ContainerNinja.Contracts.OpenApi;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatResponseQuery : IRequest<GetChatResponseVm>
    {
        public List<ChatMessageVm> PreviousMessages { get; set; } = new List<ChatMessageVm> { };
        public ChatMessageVm ChatMessage { get; set; }
        public int? ChatConversationId { get; set; }
        public string CurrentUrl { get; set; }
    }

    public class GetChatResponseQueryHandler : IRequestHandler<GetChatResponseQuery, GetChatResponseVm>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IOpenApiService _openApiService;

        public GetChatResponseQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IOpenApiService openApiService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _openApiService = openApiService;
        }

        public async Task<GetChatResponseVm> Handle(GetChatResponseQuery request, CancellationToken cancellationToken)
        {
            var chatResponseMessage = await _openApiService.GetChatResponse(request.ChatMessage.Message, request.PreviousMessages, request.CurrentUrl);

            request.PreviousMessages.Add(request.ChatMessage);
            request.PreviousMessages.Add(new ChatMessageVm
            {
                From = 1,
                Message = chatResponseMessage
            });
            ChatConversation chatConversationEntity;
            if (request.ChatConversationId.HasValue && request.ChatConversationId.Value != -1)
            {
                chatConversationEntity = _repository.ChatConversations.GetAll().Where(cc => cc.Id == request.ChatConversationId.Value).FirstOrDefault();
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
            try
            {
                var chatCommandEntity = new ChatCommand
                {
                    RawReponse = chatResponseMessage,
                    CurrentUrl = request.CurrentUrl
                };
                var startIndex = chatResponseMessage.IndexOf('{');
                var endIndex = chatResponseMessage.LastIndexOf('}');
                if (startIndex != -1 && endIndex != -1)
                {
                    chatResponseMessage = chatResponseMessage.Substring(startIndex, endIndex - startIndex + 1);
                    var openApiChatCommand = JsonConvert.DeserializeObject<OpenApiChatCommand>(chatResponseMessage);

                    chatCommandEntity.CommandName = openApiChatCommand.Cmd;
                    chatCommandEntity.ChatConversation = chatConversationEntity;
                    _repository.ChatCommands.Add(chatCommandEntity);

                    //TODO: Do we need to call SaveChangesAsync before the domain event is sent so the change tracker HasChanges only if the event modified entities?
                    //calling for now
                    //await _repository.CommitAsync();
                    //chatCommandEntity.AddDomainEvent(new ReceivedChatCommandEvent(chatCommandEntity));
                    await _repository.CommitAsync();
                    dirty = chatCommandEntity.ChangedData;
                    navigateToPage = chatCommandEntity.NavigateToPage;

                    if (!string.IsNullOrEmpty(chatCommandEntity.SystemResponse))
                    {
                        var chatSystemResponseMessage = await _openApiService.GetChatResponseFromSystem(chatCommandEntity.SystemResponse, request.PreviousMessages, request.CurrentUrl);
                        startIndex = chatSystemResponseMessage.IndexOf('{');
                        endIndex = chatSystemResponseMessage.LastIndexOf('}');

                        if (startIndex != -1 && endIndex == -1)
                        {
                            //partial json response
                            chatSystemResponseMessage = "I'm sorry, I'm having trouble contacting the server.";
                            error = true;
                        }
                        else if (startIndex != -1 && endIndex != -1)
                        {
                            //json response
                            chatSystemResponseMessage = chatSystemResponseMessage.Substring(startIndex, endIndex - startIndex + 1);
                            var openApiChatSystemResponseMessageCommand = JsonConvert.DeserializeObject<OpenApiChatCommand>(chatSystemResponseMessage);
                            chatSystemResponseMessage = openApiChatSystemResponseMessageCommand.Response;
                        }

                        request.PreviousMessages.Add(new ChatMessageVm
                        {
                            From = 3,
                            Message = chatCommandEntity.SystemResponse
                        });
                        request.PreviousMessages.Add(new ChatMessageVm
                        {
                            From = 1,
                            Message = chatSystemResponseMessage
                        });

                        chatConversationEntity.Content = JsonConvert.SerializeObject(request);
                        chatResponseMessage = chatSystemResponseMessage;
                    }
                    else
                    {
                        chatResponseMessage = openApiChatCommand.Response;
                    }
                }
            }
            catch (Exception e)
            {
                error = true;
                chatConversationEntity.Error = FlattenException(e);
                await _repository.CommitAsync();
            }

            return new GetChatResponseVm
            {
                ChatConversationId = chatConversationEntity.Id,
                Dirty = dirty,
                Error = error,
                CreateNewChat = error || !string.IsNullOrEmpty(navigateToPage),
                NavigateToPage = navigateToPage,
                ResponseMessage = new ChatMessageVm
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