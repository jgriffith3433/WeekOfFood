using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.ChatAI;
using LinqKit;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.ResponseModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Core.Services;
using ContainerNinja.Core.Handlers.Queries;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class ConsumeChatCommand : IRequest<ChatResponseVM>
    {
        public ChatConversation ChatConversation { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
        public ChatAICommand ChatAICommand { get; set; }
        public string RawChatAICommand { get; set; }
        public string CurrentUrl { get; set; }
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
                ChatConversation = request.ChatConversation,
                CommandName = request.ChatAICommand.Cmd,
            };
            _repository.ChatCommands.Add(chatCommandEntity);
            await _repository.CommitAsync();

            var chatResponseVM = new ChatResponseVM
            {
                ChatConversationId = request.ChatConversation.Id,
                ChatMessages = request.ChatMessages,
            };
            try
            {
                var result = _validator.Validate(request);

                _logger.LogInformation($"ConsumeChatCommand Validation result: {result}");

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                    throw new InvalidRequestBodyException
                    {
                        Errors = errors
                    };
                }

                var startIndex = request.RawChatAICommand.IndexOf('{');
                var endIndex = request.RawChatAICommand.LastIndexOf('}');
                var newChatMessages = new List<ChatMessage>();
                switch (request.ChatAICommand.Cmd.ToLower())
                {
                    case "go-to-page":
                        {
                            var goToPage = JsonConvert.DeserializeObject<ChatAICommandGoToPage>(request.RawChatAICommand.Substring(startIndex, endIndex - startIndex + 1));

                            if (string.IsNullOrEmpty(goToPage.Page))
                            {
                                newChatMessages.Add(ChatMessage.FromAssistant("Error: page field is required", StaticValues.ChatMessageRoles.System));
                            }
                            else
                            {
                                newChatMessages.Add(ChatMessage.FromAssistant(goToPage.Response, StaticValues.ChatMessageRoles.Assistant));
                                chatResponseVM.NavigateToPage = string.Join('-', goToPage.Page.Split(' '));
                            }
                        }
                        break;
                    case "order":
                        {
                            var chatCommandOrder = JsonConvert.DeserializeObject<ChatAICommandOrder>(request.RawChatAICommand.Substring(startIndex, endIndex - startIndex + 1));
                            newChatMessages.Add(ChatMessage.FromAssistant(chatCommandOrder.Response, StaticValues.ChatMessageRoles.Assistant));
                        }
                        break;
                    case "edit-recipe-name":
                        {
                            var editRecipeName = JsonConvert.DeserializeObject<ChatAICommandEditRecipeName>(request.RawChatAICommand.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.FirstOrDefault(r => r.Name.ToLower().Contains(editRecipeName.Original.ToLower()));
                            if (recipe == null)
                            {
                                var systemResponse = "Error: Could not find recipe by name: " + editRecipeName.Original;
                                newChatMessages.Add(ChatMessage.FromSystem(systemResponse, StaticValues.ChatMessageRoles.System));
                            }
                            else
                            {
                                recipe.Name = editRecipeName.New;
                                _repository.Recipes.Update(recipe);
                                newChatMessages.Add(ChatMessage.FromAssistant(editRecipeName.Response, StaticValues.ChatMessageRoles.Assistant));
                            }
                        }
                        break;
                    //case "substitute-recipe-ingredient":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var recipeSubstituteIngredient = JsonConvert.DeserializeObject<ChatAICommandCookedRecipeSubstituteIngredient>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock).FirstOrDefault(r => r.Name.ToLower() == recipeSubstituteIngredient.Recipe.ToLower());
                    //        if (recipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find recipe by name: " + recipeSubstituteIngredient.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(recipeSubstituteIngredient.Original.ToLower()));

                    //            if (calledIngredient == null)
                    //            {
                    //                request.ChatCommand.SystemResponse = "Error: Could not find ingredient by name: " + recipeSubstituteIngredient.Original;
                    //                request.From = 3;
                    //                request.To = 1;
                    //            }
                    //            else
                    //            {
                    //                calledIngredient.Name = recipeSubstituteIngredient.New;
                    //                calledIngredient.ProductStock = null;
                    //                _repository.CalledIngredients.Update(calledIngredient);
                    //                request.From = 1;
                    //                request.To = 2;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case "substitute-cooked-recipe-ingredient":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var cookedRecipeSubstituteIngredient = JsonConvert.DeserializeObject<ChatAICommandCookedRecipeSubstituteIngredient>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.CalledIngredient).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.ProductStock).OrderByDescending(cr => cr.Created).FirstOrDefault(cr => cr.Recipe.Name.ToLower().Contains(cookedRecipeSubstituteIngredient.Recipe.ToLower()));
                    //        if (cookedRecipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find cooked recipe by name: " + cookedRecipeSubstituteIngredient.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            if (string.IsNullOrEmpty(cookedRecipeSubstituteIngredient.Original))
                    //            {
                    //                request.ChatCommand.SystemResponse = "Error: Original property is undefined, cannot edit cooked recipe ingredient";
                    //                request.From = 3;
                    //                request.To = 1;
                    //            }
                    //            else
                    //            {
                    //                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(cookedRecipeSubstituteIngredient.Original.ToLower()));

                    //                if (cookedRecipeCalledIngredient == null)
                    //                {
                    //                    request.ChatCommand.SystemResponse = "Error: Could not find ingredient by name: " + cookedRecipeSubstituteIngredient.Original;
                    //                    request.From = 3;
                    //                    request.To = 1;
                    //                }
                    //                else
                    //                {
                    //                    cookedRecipeCalledIngredient.Name = cookedRecipeSubstituteIngredient.New;
                    //                    cookedRecipeCalledIngredient.CalledIngredient = null;
                    //                    cookedRecipeCalledIngredient.ProductStock = null;
                    //                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                    //                    request.From = 1;
                    //                    request.To = 2;
                    //                }
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case "add-recipe-ingredient":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var addRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandAddRecipeIngredient>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == addRecipeIngredient.Recipe.ToLower());
                    //        if (recipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find recipe by name: " + addRecipeIngredient.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var calledIngredient = new CalledIngredient
                    //            {
                    //                Name = addRecipeIngredient.Name,
                    //                Recipe = recipe,
                    //                Verified = false,
                    //                Units = addRecipeIngredient.Units,
                    //                UnitType = UnitTypeFromString(addRecipeIngredient.UnitType)
                    //            };
                    //            recipe.CalledIngredients.Add(calledIngredient);
                    //            _repository.CalledIngredients.Add(calledIngredient);
                    //            _repository.Recipes.Update(recipe);
                    //            request.From = 1;
                    //            request.To = 2;
                    //        }
                    //    }
                    //    break;
                    //case "add-cooked-recipe-ingredient":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var addCookedRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandAddCookedRecipeIngredient>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Name.ToLower() == addCookedRecipeIngredient.Recipe.ToLower());
                    //        if (cookedRecipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find cooked recipe by name: " + addCookedRecipeIngredient.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                    //            {
                    //                Name = addCookedRecipeIngredient.Name,
                    //                //CookedRecipe = cookedRecipe,
                    //                Units = addCookedRecipeIngredient.Units,
                    //                UnitType = UnitTypeFromString(addCookedRecipeIngredient.UnitType)
                    //            };
                    //            cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                    //            _repository.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                    //            _repository.CookedRecipes.Update(cookedRecipe);
                    //            request.From = 1;
                    //            request.To = 2;
                    //        }
                    //    }
                    //    break;
                    //case "remove-recipe-ingredient":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var deleteRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandDeleteRecipeIngredient>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == deleteRecipeIngredient.Recipe.ToLower());
                    //        if (recipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find recipe by name: " + deleteRecipeIngredient.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(deleteRecipeIngredient.Ingredient.ToLower()));

                    //            if (calledIngredient == null)
                    //            {
                    //                request.ChatCommand.SystemResponse = "Error: Could not find ingredient by name: " + deleteRecipeIngredient.Ingredient;
                    //                request.From = 3;
                    //                request.To = 1;
                    //            }
                    //            else
                    //            {
                    //                recipe.CalledIngredients.Remove(calledIngredient);
                    //                _repository.Recipes.Update(recipe);
                    //                request.From = 1;
                    //                request.To = 2;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case "remove-cooked-recipe-ingredient":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var deleteCookedRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandDeleteCookedRecipeIngredient>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).FirstOrDefault(r => r.Recipe.Name.ToLower() == deleteCookedRecipeIngredient.Recipe.ToLower());
                    //        if (cookedRecipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find cooked recipe by name: " + deleteCookedRecipeIngredient.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(deleteCookedRecipeIngredient.Ingredient.ToLower()));

                    //            if (cookedRecipeCalledIngredient == null)
                    //            {
                    //                request.ChatCommand.SystemResponse = "Error: Could not find ingredient by name: " + deleteCookedRecipeIngredient.Ingredient;
                    //                request.From = 3;
                    //                request.To = 1;
                    //            }
                    //            else
                    //            {
                    //                cookedRecipe.CookedRecipeCalledIngredients.Remove(cookedRecipeCalledIngredient);
                    //                _repository.CookedRecipes.Update(cookedRecipe);
                    //                request.From = 1;
                    //                request.To = 2;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case "edit-recipe-ingredient-unittype":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var editRecipeIngredientUnitType = JsonConvert.DeserializeObject<ChatAICommandEditRecipeIngredientUnitType>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower().Contains(editRecipeIngredientUnitType.Recipe.ToLower()));

                    //        if (recipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find recipe by name: " + editRecipeIngredientUnitType.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(editRecipeIngredientUnitType.Name.ToLower()));
                    //            if (calledIngredient == null)
                    //            {
                    //                request.ChatCommand.SystemResponse = "Error: Could not find ingredient by name: " + editRecipeIngredientUnitType.Name;
                    //                request.From = 3;
                    //                request.To = 1;
                    //            }
                    //            else
                    //            {
                    //                calledIngredient.UnitType = UnitTypeFromString(editRecipeIngredientUnitType.UnitType);
                    //                _repository.CalledIngredients.Update(calledIngredient);
                    //                request.From = 1;
                    //                request.To = 2;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case "edit-cooked-recipe-ingredient-unittype":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var editCookedRecipeIngredientUnitType = JsonConvert.DeserializeObject<ChatAICommandEditCookedRecipeIngredientUnitType>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).FirstOrDefault(r => r.Recipe.Name.ToLower().Contains(editCookedRecipeIngredientUnitType.Recipe.ToLower()));

                    //        if (cookedRecipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find cooked recipe by name: " + editCookedRecipeIngredientUnitType.Recipe;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(editCookedRecipeIngredientUnitType.Name.ToLower()));
                    //            if (cookedRecipeCalledIngredient == null)
                    //            {
                    //                request.ChatCommand.SystemResponse = "Error: Could not find ingredient by name: " + editCookedRecipeIngredientUnitType.Name;
                    //                request.From = 3;
                    //                request.To = 1;
                    //            }
                    //            else
                    //            {
                    //                cookedRecipeCalledIngredient.UnitType = UnitTypeFromString(editCookedRecipeIngredientUnitType.UnitType);
                    //                _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                    //                request.From = 1;
                    //                request.To = 2;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case "edit-product-unit-type":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var editProductUnitType = JsonConvert.DeserializeObject<ChatAICommandEditProductUnitType>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var product = _repository.Products.FirstOrDefault(p => p.Name.ToLower().Contains(editProductUnitType.Product.ToLower()));
                    //        if (product == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find product by name: " + editProductUnitType.Product;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            product.UnitType = UnitTypeFromString(editProductUnitType.UnitType);
                    //            _repository.Products.Update(product);
                    //            request.From = 1;
                    //            request.To = 2;
                    //        }
                    //    }
                    //    break;
                    //case "create-product":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var createProduct = JsonConvert.DeserializeObject<ChatAICommandCreateProduct>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var productEntity = new Product
                    //        {
                    //            Name = createProduct.Product
                    //        };

                    //        //always ensure a product stock record exists for each product
                    //        var productStockEntity = new ProductStock
                    //        {
                    //            Name = createProduct.Product,
                    //            Units = 1
                    //        };
                    //        productStockEntity.Product = productEntity;
                    //        _repository.ProductStocks.Add(productStockEntity);
                    //        request.From = 1;
                    //        request.To = 2;
                    //    }
                    //    break;
                    //case "delete-product":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var deleteProduct = JsonConvert.DeserializeObject<ChatAICommandDeleteProduct>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var predicate = PredicateBuilder.New<Product>();
                    //        var searchTerms = string.Join(' ', deleteProduct.Product.ToLower().Split('-')).Split(' ');
                    //        foreach (var searchTerm in searchTerms)
                    //        {
                    //            predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
                    //        }
                    //        var query = _repository.Products.Include<Product, ProductStock>(p => p.ProductStock)
                    //            .AsNoTracking()
                    //            .AsExpandable()
                    //            .Where(predicate).ToList();

                    //        if (query.Count > 0)
                    //        {
                    //            if (query.Count == 1 && query[0].Name.ToLower() == deleteProduct.Product.ToLower())
                    //            {
                    //                //exact match, go ahead and delete
                    //                _repository.Products.Delete(query[0].Id);
                    //                request.From = 1;
                    //                request.To = 2;
                    //            }
                    //            else
                    //            {
                    //                //unsure, ask user
                    //                var productNames = query.Select(p => p.Name).ToList();
                    //                request.ChatCommand.SystemResponse = "Which product are you referring to?\n" + string.Join(", ", productNames);
                    //                request.From = 3;
                    //                request.To = 2;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            request.ChatCommand.SystemResponse = "Cannot find product called: " + deleteProduct.Product;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //    }
                    //    break;
                    //case "create-recipe":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var createRecipe = JsonConvert.DeserializeObject<ChatAICommandCreateRecipe>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        //var response = _mediator.Send(new CreateRecipeCommand
                    //        //{
                    //        //    Name = createRecipe.Name,
                    //        //    UserImport = notification.ChatCommand.RawReponse
                    //        //});
                    //        var recipeEntity = new Recipe();

                    //        recipeEntity.Name = createRecipe.Name;
                    //        //entity.Serves = createRecipe.Serves.Value;
                    //        recipeEntity.UserImport = request.ChatCommand.RawAssistantReponse;

                    //        foreach (var createRecipeIngredient in createRecipe.Ingredients)
                    //        {
                    //            var calledIngredient = new CalledIngredient
                    //            {
                    //                Name = createRecipeIngredient.Name,
                    //                Recipe = recipeEntity,
                    //                Verified = false,
                    //                Units = createRecipeIngredient.Units,
                    //                UnitType = UnitTypeFromString(createRecipeIngredient.UnitType)
                    //            };
                    //            recipeEntity.CalledIngredients.Add(calledIngredient);
                    //            _repository.CalledIngredients.Add(calledIngredient);
                    //        }

                    //        _repository.Recipes.Add(recipeEntity);
                    //        //await _repository.CommitAsync();

                    //        //if (entity.UserImport != null)
                    //        //{
                    //        //    entity.AddDomainEvent(new RecipeUserImportEvent(entity));
                    //        //}
                    //        //await _repository.SaveChangesAsync(cancellationToken);

                    //        request.From = 1;
                    //        request.To = 2;


                    //    }
                    //    break;
                    //case "delete-recipe":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var deleteRecipe = JsonConvert.DeserializeObject<ChatAICommandDeleteRecipe>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients)
                    //            .Where(r => r.Name.ToLower() == deleteRecipe.Name.ToLower())
                    //            .SingleOrDefaultAsync(cancellationToken).Result;

                    //        if (recipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find recipe by name: " + deleteRecipe.Name;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {

                    //            foreach (var calledIngredient in recipe.CalledIngredients)
                    //            {
                    //                _repository.CalledIngredients.Delete(calledIngredient.Id);
                    //            }
                    //            _repository.Recipes.Delete(recipe.Id);
                    //            request.From = 1;
                    //            request.To = 2;
                    //        }
                    //    }
                    //    break;
                    //case "create-cooked-recipe":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var createCookedRecipe = JsonConvert.DeserializeObject<ChatAICommandCreateCookedRecipe>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));

                    //        var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock)
                    //            .Include(r => r.CookedRecipes)
                    //            .Where(r => r.Name.ToLower() == createCookedRecipe.Name.ToLower())
                    //            .SingleOrDefaultAsync(cancellationToken).Result;

                    //        if (recipe == null)
                    //        {
                    //            request.ChatCommand.SystemResponse = "Error: Could not find recipe by name: " + createCookedRecipe.Name;
                    //            request.From = 3;
                    //            request.To = 1;
                    //        }
                    //        else
                    //        {
                    //            var cookedRecipe = new CookedRecipe
                    //            {
                    //                Recipe = recipe
                    //            };
                    //            foreach (var calledIngredient in recipe.CalledIngredients)
                    //            {
                    //                var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                    //                {
                    //                    Name = calledIngredient.Name,
                    //                    CookedRecipe = cookedRecipe,
                    //                    CalledIngredient = calledIngredient,
                    //                    ProductStock = calledIngredient.ProductStock,
                    //                    UnitType = calledIngredient.UnitType,
                    //                    Units = calledIngredient.Units != null ? calledIngredient.Units.Value : 0
                    //                };
                    //                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                    //            }
                    //            recipe.CookedRecipes.Add(cookedRecipe);
                    //            _repository.Recipes.Update(recipe);
                    //            _repository.CookedRecipes.Add(cookedRecipe);
                    //            request.From = 1;
                    //            request.To = 2;
                    //        }

                    //        break;
                    //    }
                    //case "none":
                    //    {
                    //        var startIndex = request.ChatCommand.RawAssistantReponse.IndexOf('{');
                    //        var endIndex = request.ChatCommand.RawAssistantReponse.LastIndexOf('}');
                    //        var none = JsonConvert.DeserializeObject<ChatAICommandNone>(request.ChatCommand.RawAssistantReponse.Substring(startIndex, endIndex - startIndex + 1));
                    //        request.From = 1;
                    //        request.To = 2;
                    //    }
                    //    break;
                    default:
                        {
                            if (string.IsNullOrEmpty(request.ChatAICommand.Cmd))
                            {
                                newChatMessages.Add(ChatMessage.FromSystem("cmd is required", StaticValues.ChatMessageRoles.System));
                            }
                            else
                            {
                                newChatMessages.Add(ChatMessage.FromSystem("Unknown cmd", StaticValues.ChatMessageRoles.System));
                                chatCommandEntity.Unknown = true;
                                //Commiting in switch case prevents dirty flag from being thrown
                                await _repository.CommitAsync();
                            }
                        }
                        break;
                }
                chatResponseVM.ChatMessages.AddRange(newChatMessages);
                chatResponseVM.Dirty = _repository.ChangeTracker.HasChanges();
                chatCommandEntity.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages);
                chatCommandEntity.NavigateToPage = chatResponseVM.NavigateToPage;
                chatResponseVM.CreateNewChat = !string.IsNullOrEmpty(chatResponseVM.NavigateToPage);
                chatCommandEntity.ChangedData = chatResponseVM.Dirty;

                await _repository.CommitAsync();
            }
            catch (Exception ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                chatResponseVM.CreateNewChat = true;
                chatResponseVM.ChatMessages.Add(ChatMessage.FromSystem(ex.Message, StaticValues.ChatMessageRoles.System));
                chatCommandEntity.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages);
                await _repository.CommitAsync();
            }

            return chatResponseVM;
        }

        private UnitType UnitTypeFromString(string unitTypeStr)
        {
            switch (unitTypeStr.ToLower())
            {
                case "":
                case "none":
                    return UnitType.None;
                case "bulk":
                    return UnitType.Bulk;
                case "ounce":
                case "ounces":
                    return UnitType.Ounce;
                case "teaspoon":
                case "teaspoons":
                    return UnitType.Teaspoon;
                case "tablespoon":
                case "tablespoons":
                    return UnitType.Tablespoon;
                case "pound":
                case "pounds":
                    return UnitType.Pound;
                case "cup":
                case "cups":
                    return UnitType.Cup;
                case "clove":
                case "cloves":
                    return UnitType.Cloves;
                case "can":
                case "cans":
                    return UnitType.Can;
                case "whole":
                case "wholes":
                    return UnitType.Whole;
                case "package":
                case "packages":
                    return UnitType.Package;
                case "bar":
                case "bars":
                    return UnitType.Bar;
                case "bun":
                case "buns":
                    return UnitType.Bun;
                case "bottle":
                case "bottles":
                    return UnitType.Bottle;
                default:
                    return UnitType.None;
            }
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