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

namespace ContainerNinja.Core.Handlers.Commands
{
    public class ReceivedChatCommand : IRequest<bool>
    {
        public ChatCommand ChatCommand { get; set; }
    }

    public class ReceivedChatCommandHandler : IRequestHandler<ReceivedChatCommand, bool>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<ReceivedChatCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<ReceivedChatCommandHandler> _logger;
        private readonly ICachingService _cache;

        public ReceivedChatCommandHandler(ILogger<ReceivedChatCommandHandler> logger, IUnitOfWork repository, IValidator<ReceivedChatCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<bool> Handle(ReceivedChatCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = _validator.Validate(request);

                _logger.LogInformation($"ReceivedChatCommand Validation result: {result}");

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                    throw new InvalidRequestBodyException
                    {
                        Errors = errors
                    };
                }
                string? systemResponse = null;
                switch (request.ChatCommand.CommandName?.ToLower())
                {
                    case "order":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var chatCommandOrder = JsonConvert.DeserializeObject<ChatAICommandOrder>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                        }
                        break;
                    case "edit-recipe-name":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var editRecipeName = JsonConvert.DeserializeObject<ChatAICommandEditRecipeName>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.FirstOrDefault(r => r.Name.ToLower().Contains(editRecipeName.Original.ToLower()));
                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + editRecipeName.Original;
                            }
                            else
                            {
                                recipe.Name = editRecipeName.New;
                            }
                            _repository.Recipes.Update(recipe);
                        }
                        break;
                    case "substitute-recipe-ingredient":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var recipeSubstituteIngredient = JsonConvert.DeserializeObject<ChatAICommandCookedRecipeSubstituteIngredient>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));


                            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock).FirstOrDefault(r => r.Name.ToLower() == recipeSubstituteIngredient.Recipe.ToLower());
                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + recipeSubstituteIngredient.Recipe;
                            }
                            else
                            {
                                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(recipeSubstituteIngredient.Original.ToLower()));

                                if (calledIngredient == null)
                                {
                                    systemResponse = "Error: Could not find ingredient by name: " + recipeSubstituteIngredient.Original;
                                }
                                else
                                {
                                    calledIngredient.Name = recipeSubstituteIngredient.New;
                                    calledIngredient.ProductStock = null;
                                    _repository.CalledIngredients.Update(calledIngredient);
                                }
                            }
                        }
                        break;
                    case "substitute-cooked-recipe-ingredient":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var cookedRecipeSubstituteIngredient = JsonConvert.DeserializeObject<ChatAICommandCookedRecipeSubstituteIngredient>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));


                            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, Recipe>(cr => cr.Recipe).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.CalledIngredient).Include(cr => cr.CookedRecipeCalledIngredients).ThenInclude(crci => crci.ProductStock).OrderByDescending(cr => cr.Created).FirstOrDefault(cr => cr.Recipe.Name.ToLower().Contains(cookedRecipeSubstituteIngredient.Recipe.ToLower()));
                            if (cookedRecipe == null)
                            {
                                systemResponse = "Error: Could not find cooked recipe by name: " + cookedRecipeSubstituteIngredient.Recipe;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(cookedRecipeSubstituteIngredient.Original))
                                {
                                    systemResponse = "Error: Original property is undefined, cannot edit cooked recipe ingredient";
                                }
                                else
                                {
                                    var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(cookedRecipeSubstituteIngredient.Original.ToLower()));

                                    if (cookedRecipeCalledIngredient == null)
                                    {
                                        systemResponse = "Error: Could not find ingredient by name: " + cookedRecipeSubstituteIngredient.Original;
                                    }
                                    else
                                    {
                                        cookedRecipeCalledIngredient.Name = cookedRecipeSubstituteIngredient.New;
                                        cookedRecipeCalledIngredient.CalledIngredient = null;
                                        cookedRecipeCalledIngredient.ProductStock = null;
                                        _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                                    }
                                }
                            }
                        }
                        break;
                    case "add-recipe-ingredient":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var addRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandAddRecipeIngredient>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == addRecipeIngredient.Recipe.ToLower());
                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + addRecipeIngredient.Recipe;
                            }
                            else
                            {
                                var calledIngredient = new CalledIngredient
                                {
                                    Name = addRecipeIngredient.Name,
                                    Recipe = recipe,
                                    Verified = false,
                                    Units = addRecipeIngredient.Units,
                                    UnitType = UnitTypeFromString(addRecipeIngredient.UnitType)
                                };
                                recipe.CalledIngredients.Add(calledIngredient);
                                _repository.CalledIngredients.Add(calledIngredient);
                                _repository.Recipes.Update(recipe);
                            }
                        }
                        break;
                    case "add-cooked-recipe-ingredient":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var addCookedRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandAddCookedRecipeIngredient>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Name.ToLower() == addCookedRecipeIngredient.Recipe.ToLower());
                            if (cookedRecipe == null)
                            {
                                systemResponse = "Error: Could not find cooked recipe by name: " + addCookedRecipeIngredient.Recipe;
                            }
                            else
                            {
                                var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                                {
                                    Name = addCookedRecipeIngredient.Name,
                                    //CookedRecipe = cookedRecipe,
                                    Units = addCookedRecipeIngredient.Units,
                                    UnitType = UnitTypeFromString(addCookedRecipeIngredient.UnitType)
                                };
                                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                                _repository.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                                _repository.CookedRecipes.Update(cookedRecipe);
                            }
                        }
                        break;
                    case "remove-recipe-ingredient":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var deleteRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandDeleteRecipeIngredient>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower() == deleteRecipeIngredient.Recipe.ToLower());
                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + deleteRecipeIngredient.Recipe;
                            }
                            else
                            {
                                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(deleteRecipeIngredient.Ingredient.ToLower()));

                                if (calledIngredient == null)
                                {
                                    systemResponse = "Error: Could not find ingredient by name: " + deleteRecipeIngredient.Ingredient;
                                }
                                else
                                {
                                    recipe.CalledIngredients.Remove(calledIngredient);
                                    _repository.Recipes.Update(recipe);
                                }
                            }
                        }
                        break;
                    case "remove-cooked-recipe-ingredient":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var deleteCookedRecipeIngredient = JsonConvert.DeserializeObject<ChatAICommandDeleteCookedRecipeIngredient>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).FirstOrDefault(r => r.Recipe.Name.ToLower() == deleteCookedRecipeIngredient.Recipe.ToLower());
                            if (cookedRecipe == null)
                            {
                                systemResponse = "Error: Could not find cooked recipe by name: " + deleteCookedRecipeIngredient.Recipe;
                            }
                            else
                            {
                                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(deleteCookedRecipeIngredient.Ingredient.ToLower()));

                                if (cookedRecipeCalledIngredient == null)
                                {
                                    systemResponse = "Error: Could not find ingredient by name: " + deleteCookedRecipeIngredient.Ingredient;
                                }
                                else
                                {
                                    cookedRecipe.CookedRecipeCalledIngredients.Remove(cookedRecipeCalledIngredient);
                                    _repository.CookedRecipes.Update(cookedRecipe);
                                }
                            }
                        }
                        break;
                    case "edit-recipe-ingredient-unittype":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var editRecipeIngredientUnitType = JsonConvert.DeserializeObject<ChatAICommandEditRecipeIngredientUnitType>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).FirstOrDefault(r => r.Name.ToLower().Contains(editRecipeIngredientUnitType.Recipe.ToLower()));

                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + editRecipeIngredientUnitType.Recipe;
                            }
                            else
                            {
                                var calledIngredient = recipe.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(editRecipeIngredientUnitType.Name.ToLower()));
                                if (calledIngredient == null)
                                {
                                    systemResponse = "Error: Could not find ingredient by name: " + editRecipeIngredientUnitType.Name;
                                }
                                else
                                {
                                    calledIngredient.UnitType = UnitTypeFromString(editRecipeIngredientUnitType.UnitType);
                                    _repository.CalledIngredients.Update(calledIngredient);
                                }
                            }
                        }
                        break;
                    case "edit-cooked-recipe-ingredient-unittype":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var editCookedRecipeIngredientUnitType = JsonConvert.DeserializeObject<ChatAICommandEditCookedRecipeIngredientUnitType>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var cookedRecipe = _repository.CookedRecipes.Include<CookedRecipe, IList<CookedRecipeCalledIngredient>>(r => r.CookedRecipeCalledIngredients).FirstOrDefault(r => r.Recipe.Name.ToLower().Contains(editCookedRecipeIngredientUnitType.Recipe.ToLower()));

                            if (cookedRecipe == null)
                            {
                                systemResponse = "Error: Could not find cooked recipe by name: " + editCookedRecipeIngredientUnitType.Recipe;
                            }
                            else
                            {
                                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(editCookedRecipeIngredientUnitType.Name.ToLower()));
                                if (cookedRecipeCalledIngredient == null)
                                {
                                    systemResponse = "Error: Could not find ingredient by name: " + editCookedRecipeIngredientUnitType.Name;
                                }
                                else
                                {
                                    cookedRecipeCalledIngredient.UnitType = UnitTypeFromString(editCookedRecipeIngredientUnitType.UnitType);
                                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                                }
                            }
                        }
                        break;
                    case "edit-product-unit-type":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var editProductUnitType = JsonConvert.DeserializeObject<ChatAICommandEditProductUnitType>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var product = _repository.Products.FirstOrDefault(p => p.Name.ToLower().Contains(editProductUnitType.Product.ToLower()));
                            if (product == null)
                            {
                                systemResponse = "Error: Could not find product by name: " + editProductUnitType.Product;
                            }
                            else
                            {
                                product.UnitType = UnitTypeFromString(editProductUnitType.UnitType);
                                _repository.Products.Update(product);
                            }
                        }
                        break;
                    case "create-recipe":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var createRecipe = JsonConvert.DeserializeObject<ChatAICommandCreateRecipe>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            //var response = _mediator.Send(new CreateRecipeCommand
                            //{
                            //    Name = createRecipe.Name,
                            //    UserImport = notification.ChatCommand.RawReponse
                            //});
                            var recipeEntity = new Recipe();

                            recipeEntity.Name = createRecipe.Name;
                            //entity.Serves = createRecipe.Serves.Value;
                            recipeEntity.UserImport = request.ChatCommand.RawReponse;

                            foreach (var createRecipeIngredient in createRecipe.Ingredients)
                            {
                                var calledIngredient = new CalledIngredient
                                {
                                    Name = createRecipeIngredient.Name,
                                    Recipe = recipeEntity,
                                    Verified = false,
                                    Units = createRecipeIngredient.Units,
                                    UnitType = UnitTypeFromString(createRecipeIngredient.UnitType)
                                };
                                recipeEntity.CalledIngredients.Add(calledIngredient);
                                _repository.CalledIngredients.Add(calledIngredient);
                            }

                            _repository.Recipes.Add(recipeEntity);
                            //await _repository.CommitAsync();

                            //if (entity.UserImport != null)
                            //{
                            //    entity.AddDomainEvent(new RecipeUserImportEvent(entity));
                            //}
                            //await _repository.SaveChangesAsync(cancellationToken);



                        }
                        break;
                    case "delete-recipe":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var deleteRecipe = JsonConvert.DeserializeObject<ChatAICommandDeleteRecipe>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients)
                                .Where(r => r.Name.ToLower() == deleteRecipe.Name.ToLower())
                                .SingleOrDefaultAsync(cancellationToken).Result;

                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + deleteRecipe.Name;
                            }
                            else
                            {

                                foreach (var calledIngredient in recipe.CalledIngredients)
                                {
                                    _repository.CalledIngredients.Delete(calledIngredient.Id);
                                }
                                _repository.Recipes.Delete(recipe.Id);
                            }
                        }
                        break;
                    case "create-cooked-recipe":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var createCookedRecipe = JsonConvert.DeserializeObject<ChatAICommandCreateCookedRecipe>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            var recipe = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(r => r.CalledIngredients).ThenInclude(ci => ci.ProductStock)
                                .Include(r => r.CookedRecipes)
                                .Where(r => r.Name.ToLower() == createCookedRecipe.Name.ToLower())
                                .SingleOrDefaultAsync(cancellationToken).Result;

                            if (recipe == null)
                            {
                                systemResponse = "Error: Could not find recipe by name: " + createCookedRecipe.Name;
                            }
                            else
                            {
                                var cookedRecipe = new CookedRecipe
                                {
                                    Recipe = recipe
                                };
                                foreach (var calledIngredient in recipe.CalledIngredients)
                                {
                                    var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                                    {
                                        Name = calledIngredient.Name,
                                        CookedRecipe = cookedRecipe,
                                        CalledIngredient = calledIngredient,
                                        ProductStock = calledIngredient.ProductStock,
                                        UnitType = calledIngredient.UnitType,
                                        Units = calledIngredient.Units != null ? calledIngredient.Units.Value : 0
                                    };
                                    cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
                                }
                                recipe.CookedRecipes.Add(cookedRecipe);
                                _repository.Recipes.Update(recipe);
                                _repository.CookedRecipes.Add(cookedRecipe);
                            }

                            break;
                        }
                    case "go-to-page":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var goToPage = JsonConvert.DeserializeObject<ChatAICommandGoToPage>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));

                            if (string.IsNullOrEmpty(goToPage.Page))
                            {
                                systemResponse = "Error: page field is required";
                            }
                            else
                            {
                                request.ChatCommand.NavigateToPage = string.Join('-', goToPage.Page.Split(' '));
                            }
                        }
                        break;
                    case "none":
                        {
                            var startIndex = request.ChatCommand.RawReponse.IndexOf('{');
                            var endIndex = request.ChatCommand.RawReponse.LastIndexOf('}');
                            var none = JsonConvert.DeserializeObject<ChatAICommandNone>(request.ChatCommand.RawReponse.Substring(startIndex, endIndex - startIndex + 1));
                        }
                        break;
                    default:
                        {
                            request.ChatCommand.Unknown = true;
                            if (string.IsNullOrEmpty(request.ChatCommand.CommandName))
                            {
                                systemResponse = "cmd is required";
                            }
                            else
                            {
                                systemResponse = "Unknown cmd";
                            }
                        }
                        break;
                }
                request.ChatCommand.ChangedData = _repository.ChangeTracker.HasChanges();
                request.ChatCommand.SystemResponse = systemResponse;
                await _repository.CommitAsync();
            }
            catch (Exception ex)
            {
                request.ChatCommand.Error = FlattenException(ex);
                request.ChatCommand.SystemResponse = "Error: " + ex.Message;
                await _repository.CommitAsync();
            }
            return string.IsNullOrEmpty(request.ChatCommand.Error) && string.IsNullOrEmpty(request.ChatCommand.SystemResponse);
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