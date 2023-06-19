using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;
using System.Text;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels;
using System.Reflection;
using ContainerNinja.Core.Common;
using System.Text.Json;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Google.Api;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommand : IRequest<ChatResponseVM>
    {
        public ChatConversation ChatConversation { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatMessageVM? CurrentChatMessage { get; set; }
        public string CurrentUrl { get; set; }
        public string NavigateToPage { get; set; }
        public bool Dirty { get; set; }
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
            var chatAICommandName = request.CurrentChatMessage.FunctionCall.Value.GetProperty("name").GetString();
            var chatCommandEntity = _repository.ChatCommands.CreateProxy();
            {
                chatCommandEntity.RawChatAICommand = JsonSerializer.Serialize(request.CurrentChatMessage);
                chatCommandEntity.CurrentUrl = request.CurrentUrl;
                chatCommandEntity.CommandName = chatAICommandName;
                chatCommandEntity.ChatConversationId = request.ChatConversation.Id;
            };
            _repository.ChatCommands.Add(chatCommandEntity);
            request.ChatConversation.ChatCommands.Add(chatCommandEntity);
            await _repository.CommitAsync();
            var chatResponseVM = new ChatResponseVM
            {
                ChatConversationId = request.ChatConversation.Id,
                ChatMessages = request.ChatMessages,
                NavigateToPage = request.NavigateToPage,
                Dirty = request.Dirty,
            };
            try
            {
                if (!string.IsNullOrEmpty(chatAICommandName))
                {
                    var chatCommandConsumerType = GetChatCommandConsumerType(string.Join("_", chatAICommandName.ToLower().Split(" ")));
                    if (chatCommandConsumerType != null)
                    {
                        var chatCommandConsumer = Activator.CreateInstance(chatCommandConsumerType);
                        if (chatCommandConsumer != null)
                        {
                            chatCommandConsumerType.GetProperty("Command")?.SetValue(chatCommandConsumer, JsonSerializer.Deserialize(request.CurrentChatMessage.FunctionCall.Value.GetProperty("arguments").GetString(), GetChatAICommandModelTypeFromConsumerType(chatCommandConsumerType), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                AllowTrailingCommas = true,
                            }));
                            chatCommandConsumerType.GetProperty("Response")?.SetValue(chatCommandConsumer, chatResponseVM);

                            var result = await _mediator.Send(chatCommandConsumer, cancellationToken) as string;
                            request.CurrentChatMessage.Content = result;
                            request.CurrentChatMessage.FunctionCall = null;
                            request.CurrentChatMessage.Name = chatAICommandName;
                            request.CurrentChatMessage.To = StaticValues.ChatMessageRoles.Assistant;
                            request.CurrentChatMessage.From = StaticValues.ChatMessageRoles.Function;
                            request.CurrentChatMessage.Received = false;
                            //chatResponseVM.ChatMessages.Add(new ChatMessageVM
                            //{
                            //    Content = result,
                            //    From = StaticValues.ChatMessageRoles.Function,
                            //    To = StaticValues.ChatMessageRoles.Assistant,
                            //    Name = chatAICommandName,
                            //});
                            if (string.IsNullOrEmpty(chatResponseVM.ForceFunctionCall))
                            {
                                chatResponseVM.ForceFunctionCall = "auto";
                            }
                        }
                    }
                    else
                    {
                        throw new ChatAIException("Unknown command: " + chatAICommandName);
                    }
                }
                chatCommandEntity.ChangedData = chatResponseVM.Dirty;
                chatCommandEntity.UnknownCommand = chatResponseVM.UnknownCommand;
                chatCommandEntity.NavigateToPage = chatResponseVM.NavigateToPage;
                request.ChatConversation.Content = JsonSerializer.Serialize(chatResponseVM.ChatMessages);
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (ApiValidationException ex)
            {
                var errors = "";

                foreach (var error in ex.Errors)
                {
                    var validationMessage = "";
                    foreach (var v in error.Value)
                    {
                        if (!v.Contains("ForceFunctionCall="))
                        {
                            validationMessage += "\n" + v;
                        }
                    }
                    if (!string.IsNullOrEmpty(validationMessage))
                    {
                        errors += "\n" + error.Key + ":" + validationMessage;
                    }
                }

                chatCommandEntity.Error = FlattenException(ex) + "\n" + errors;
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = string.IsNullOrEmpty(errors) ? "More information required" : "Validation:\n" + errors,
                    From = StaticValues.ChatMessageRoles.System,
                    To = StaticValues.ChatMessageRoles.Assistant,
                    Name = chatAICommandName,
                });
                chatResponseVM.ForceFunctionCall = ex.ForceFunctionCall;
                request.ChatConversation.Content = JsonSerializer.Serialize(request.ChatMessages);
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (ChatAIException ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = ex.Message,
                    From = StaticValues.ChatMessageRoles.Function,
                    To = StaticValues.ChatMessageRoles.Assistant,
                    Name = chatAICommandName,
                });
                chatResponseVM.ForceFunctionCall = ex.ForceFunctionCall;
                request.ChatConversation.Content = JsonSerializer.Serialize(request.ChatMessages);
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _repository.ChangeTracker.Clear();
                chatCommandEntity.Error = FlattenException(ex);
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = chatCommandEntity.Error,
                    From = StaticValues.ChatMessageRoles.Function,
                    To = StaticValues.ChatMessageRoles.Assistant,
                    Name = chatAICommandName,
                });
                chatResponseVM.ForceFunctionCall = "none";
                request.ChatConversation.Content = JsonSerializer.Serialize(chatResponseVM.ChatMessages);
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (Exception ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = chatCommandEntity.Error,
                    From = StaticValues.ChatMessageRoles.Function,
                    To = StaticValues.ChatMessageRoles.Assistant,
                    Name = chatAICommandName,
                });
                chatResponseVM.ForceFunctionCall = "none";
                request.ChatConversation.Content = JsonSerializer.Serialize(chatResponseVM.ChatMessages);
                _repository.ChatCommands.Update(chatCommandEntity);
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