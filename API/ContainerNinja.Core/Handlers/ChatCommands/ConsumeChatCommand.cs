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
using ContainerNinja.Core.Handlers.Queries;
using OpenAI.ObjectModels.ResponseModels;

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
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumeChatCommandHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public ConsumeChatCommandHandler(ILogger<ConsumeChatCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
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
                switch (string.Join("_", request.ChatAICommand.Cmd.ToLower().Split(" ")))
                {
                    case "go_to":
                    case "go_to_page":
                    case "navigate":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandGoToPage>(request));
                        }
                        break;
                    case "go_to_recipes":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandGoToRecipes>(request));
                        }
                        break;
                    case "order":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandOrder>(request));
                        }
                        break;
                    case "edit_recipe_name":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditRecipeName>(request));
                        }
                        break;
                    case "substitute_recipe_ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandRecipeSubstituteIngredient>(request));
                        }
                        break;
                    case "substitute_cooked_recipe_ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCookedRecipeSubstituteIngredient>(request));
                        }
                        break;
                    case "add_recipe_ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandAddRecipeIngredient>(request));
                        }
                        break;
                    case "add_cooked_recipe_ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandAddCookedRecipeIngredient>(request));
                        }
                        break;
                    case "remove_recipe_ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteRecipeIngredient>(request));
                        }
                        break;
                    case "remove_cooked_recipe_ingredient":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteCookedRecipeIngredient>(request));
                        }
                        break;
                    case "edit_recipe_ingredient_unittype":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditRecipeIngredientUnitType>(request));
                        }
                        break;
                    case "edit_cooked_recipe_ingredient_unittype":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditCookedRecipeIngredientUnitType>(request));
                        }
                        break;
                    case "edit_product_unit_type":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandEditProductUnitType>(request));
                        }
                        break;
                    case "create_product":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCreateProduct>(request));
                        }
                        break;
                    case "delete_product":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteProduct>(request));
                        }
                        break;
                    case "create_recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCreateRecipe>(request));
                        }
                        break;
                    case "delete_recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandDeleteRecipe>(request));
                        }
                        break;
                    case "create_cooked_recipe":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandCreateCookedRecipe>(request));
                            break;
                        }
                    case "none":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandNone>(request));
                        }
                        break;
                    case "delete_cooked_recipe":
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
                //chatResponseVM.CreateNewChat = !string.IsNullOrEmpty(chatResponseVM.NavigateToPage);
                chatResponseVM.ChatConversationId = request.ChatConversation.Id;
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
                };
                //chatResponseVM.CreateNewChat = true;
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