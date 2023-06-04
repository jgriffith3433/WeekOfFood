using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using System.Text;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.ChatAI;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommand : IRequest<ChatResponseVM>
    {
        public ChatConversation ChatConversation { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatAICommand ChatAICommand { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
        public int CurrentSystemToAssistantChatCalls { get; set; }
    }

    public class ConsumeChatCommandHandler : IRequestHandler<ConsumeChatCommand, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<ConsumeChatCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandHandler(ILogger<ConsumeChatCommandHandler> logger, IUnitOfWork repository, IValidator<ConsumeChatCommand> validator, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _chatAIService = chatAIService;
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
            ChatResponseVM chatResponseVM;
            try
            {
                var result = _validator.Validate(request);

                _logger.LogInformation($"Validation result: {result}");

                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }

                switch (request.ChatAICommand.Cmd.ToLower())
                {
                    case "go-to-page":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandGoToPage>(request));
                        }
                        break;
                    case "order":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandOrder>(request));
                        }
                        break;
                    case "edit-recipe-name":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditRecipeName>(request));
                        }
                        break;
                    case "substitute-recipe-ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandRecipeSubstituteIngredient>(request));
                        }
                        break;
                    case "substitute-cooked-recipe-ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCookedRecipeSubstituteIngredient>(request));
                        }
                        break;
                    case "add-recipe-ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandAddRecipeIngredient>(request));
                        }
                        break;
                    case "add-cooked-recipe-ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandAddCookedRecipeIngredient>(request));
                        }
                        break;
                    case "remove-recipe-ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteRecipeIngredient>(request));
                        }
                        break;
                    case "remove-cooked-recipe-ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteCookedRecipeIngredient>(request));
                        }
                        break;
                    case "edit-recipe-ingredient-unittype":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditRecipeIngredientUnitType>(request));
                        }
                        break;
                    case "edit-cooked-recipe-ingredient-unittype":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditCookedRecipeIngredientUnitType>(request));
                        }
                        break;
                    case "edit-product-unit-type":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditProductUnitType>(request));
                        }
                        break;
                    case "create-product":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCreateProduct>(request));
                        }
                        break;
                    case "delete-product":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteProduct>(request));
                        }
                        break;
                    case "create-recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCreateRecipe>(request));
                        }
                        break;
                    case "delete-recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteRecipe>(request));
                        }
                        break;
                    case "create-cooked-recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCreateCookedRecipe>(request));
                            break;
                        }
                    case "none":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandNone>(request));
                        }
                        break;
                    case "delete-cooked-recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteCookedRecipe>(request));
                        }
                        break;
                    default:
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDefault>(request));
                        }
                        break;
                }
                chatResponseVM.Dirty = _repository.ChangeTracker.HasChanges();
                chatCommandEntity.ChangedData = chatResponseVM.Dirty;
                chatCommandEntity.UnknownCommand = chatResponseVM.UnknownCommand;
                chatCommandEntity.NavigateToPage = chatResponseVM.NavigateToPage;
                chatResponseVM.CreateNewChat = !string.IsNullOrEmpty(chatResponseVM.NavigateToPage);
                chatResponseVM.ChatConversationId = request.ChatConversation.Id;
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages);

                await _repository.CommitAsync();
            }
            catch (Exception ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                chatResponseVM = new ChatResponseVM
                {
                    ChatMessages = request.ChatMessages,
                };
                chatResponseVM.CreateNewChat = true;
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = ex.Message,
                    RawContent = ex.Message,
                    Name = StaticValues.ChatMessageRoles.System
                });
                chatResponseVM.ChatConversationId = request.ChatConversation.Id;
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages);

                await _repository.CommitAsync();
            }
            _cache.Clear();

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