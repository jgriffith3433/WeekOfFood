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
                    case "none":
                        {
                            chatResponseVM = await _mediator.Send(_mapper.Map<ConsumeChatCommandNone>(request));
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