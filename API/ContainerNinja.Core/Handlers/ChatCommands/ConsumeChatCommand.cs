using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using System.Text;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels;
using System.Reflection;
using ContainerNinja.Core.Common;
using AutoMapper.Internal;
using ContainerNinja.Contracts.DTO.ChatAICommands;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommand : IRequest<ChatResponseVM>
    {
        public ChatConversation ChatConversation { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatAICommandDTO ChatAICommand { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
    }

    public class ConsumeChatCommandHandler : IRequestHandler<ConsumeChatCommand, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;
        private readonly IMediator _mediator;

        public ConsumeChatCommandHandler(IUnitOfWork repository, ICachingService cache, IMediator mediator)
        {
            _repository = repository;
            _cache = cache;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommand request, CancellationToken cancellationToken)
        {
            var chatCommandEntity = new ChatCommand
            {
                RawChatAICommand = request.RawChatAICommand,
                CurrentUrl = request.CurrentUrl,
                CommandName = request.ChatAICommand.Cmd,
                ChatConversationId = request.ChatConversation.Id
            };
            _repository.ChatCommands.Add(chatCommandEntity);
            request.ChatConversation.ChatCommands.Add(chatCommandEntity);
            await _repository.CommitAsync();
            ChatResponseVM? chatResponseVM = null;
            try
            {
                if (!string.IsNullOrEmpty(request.ChatAICommand.Cmd))
                {
                    var chatCommandConsumerType = GetChatCommandConsumerType(string.Join("_", request.ChatAICommand.Cmd.ToLower().Split(" ")));
                    if (chatCommandConsumerType != null)
                    {
                        var chatCommandConsumer = Activator.CreateInstance(chatCommandConsumerType);
                        if (chatCommandConsumer != null)
                        {
                            chatCommandConsumerType.GetProperty("Command")?.SetValue(chatCommandConsumer, JsonConvert.DeserializeObject(request.RawChatAICommand.Substring(request.RawChatAICommand.IndexOf('{'), request.RawChatAICommand.LastIndexOf('}') - request.RawChatAICommand.IndexOf('{') + 1), GetChatAICommandModelTypeFromConsumerType(chatCommandConsumerType)));
                            chatCommandConsumerType.GetProperty("Response")?.SetValue(chatCommandConsumer, new ChatResponseVM { ChatConversationId = request.ChatConversation.Id, ChatMessages = request.ChatMessages });

                            chatResponseVM = await _mediator.Send(chatCommandConsumer, cancellationToken) as ChatResponseVM;
                        }
                    }
                }
                if (chatResponseVM == null)
                {
                    var chatCommandModel = JsonConvert.DeserializeObject<ChatAICommandDTOUnknown>(request.RawChatAICommand.Substring(request.RawChatAICommand.IndexOf('{'), request.RawChatAICommand.LastIndexOf('}') - request.RawChatAICommand.IndexOf('{') + 1));
                    if (chatCommandModel != null)
                    {
                        var chatCommandConsumer = new ConsumeChatCommandUnknown
                        {
                            Command = chatCommandModel,
                            Response = chatResponseVM = new ChatResponseVM { ChatConversationId = request.ChatConversation.Id, ChatMessages = request.ChatMessages, UnknownCommand = true },
                        };
                        chatResponseVM = await _mediator.Send(chatCommandConsumer);
                    }
                }
                chatResponseVM.Dirty = _repository.ChangeTracker.HasChanges();
                //chatResponseVM.CreateNewChat = !string.IsNullOrEmpty(chatResponseVM.NavigateToPage);
                chatCommandEntity.ChangedData = chatResponseVM.Dirty;
                chatCommandEntity.UnknownCommand = chatResponseVM.UnknownCommand;
                chatCommandEntity.NavigateToPage = chatResponseVM.NavigateToPage;
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages);

                await _repository.CommitAsync();
            }
            catch (ApiValidationException ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                request.ChatConversation.Content = JsonConvert.SerializeObject(request.ChatMessages);
                await _repository.CommitAsync();
                throw ex;
            }
            catch (ChatAIException ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                request.ChatConversation.Content = JsonConvert.SerializeObject(request.ChatMessages);
                await _repository.CommitAsync();
                throw ex;
            }
            catch (Exception ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                chatResponseVM = new ChatResponseVM
                {
                    ChatMessages = request.ChatMessages,
                    ChatConversationId = request.ChatConversation.Id
                };
                //chatResponseVM.CreateNewChat = true;
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = ex.Message,
                    RawContent = ex.Message,
                    Name = StaticValues.ChatMessageRoles.System
                });
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages);

                await _repository.CommitAsync();
            }
            _cache.Clear();

            return chatResponseVM;
        }

        public static Type? GetChatCommandConsumerType(string commandName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ChatCommandModel>() != null && x.GetCustomAttribute<ChatCommandModel>().CommandNames.Contains(commandName)).FirstOrDefault();
        }

        public static Type? GetChatAICommandModelTypeFromConsumerType(Type consumerType)
        {
            return consumerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IChatCommandConsumer<>)).SelectMany(i => i.GetGenericArguments()).FirstOrDefault();
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