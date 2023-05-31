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

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateCookedRecipeCommand : IRequest<CookedRecipeDTO>
    {
        public int? RecipeId { get; init; }
    }

    public class CreateCookedRecipeCommandHandler : IRequestHandler<CreateCookedRecipeCommand, CookedRecipeDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<CreateCookedRecipeCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCookedRecipeCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateCookedRecipeCommandHandler(ILogger<CreateCookedRecipeCommandHandler> logger, IUnitOfWork repository, IValidator<CreateCookedRecipeCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<CookedRecipeDTO> Handle(CreateCookedRecipeCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);

            _logger.LogInformation($"CreateCookedRecipeCommand Validation result: {result}");

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                throw new InvalidRequestBodyException
                {
                    Errors = errors
                };
            }

            var recipeEntity = _repository.Recipes.Include<Recipe, IList<CalledIngredient>>(p => p.CalledIngredients).ThenInclude(ci => ci.ProductStock).Include(r => r.CookedRecipes).FirstOrDefault(r => r.Id == request.RecipeId);

            if (recipeEntity == null)
            {
                throw new EntityNotFoundException($"No Recipe found for the Id {request.RecipeId}");
            }

            var cookedRecipeEntity = new CookedRecipe();

            cookedRecipeEntity.Recipe = recipeEntity;
            recipeEntity.CookedRecipes.Add(cookedRecipeEntity);
            foreach (var calledIngredient in recipeEntity.CalledIngredients)
            {
                var cookedRecipeCalledIngredient = new CookedRecipeCalledIngredient
                {
                    CookedRecipe = cookedRecipeEntity,
                    CalledIngredient = calledIngredient,
                    Name = calledIngredient.Name,
                    UnitType = calledIngredient.UnitType,
                    Units = calledIngredient.Units,
                    ProductStock = calledIngredient.ProductStock
                };
                cookedRecipeEntity.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
            }
            _repository.Recipes.Update(recipeEntity);
            _repository.CookedRecipes.Add(cookedRecipeEntity);

            await _repository.CommitAsync();

            var cookedRecipeDTO = _mapper.Map<CookedRecipeDTO>(cookedRecipeEntity);
            _cache.SetItem($"cooked_recipe_{cookedRecipeDTO.Id}", cookedRecipeDTO);
            _cache.RemoveItem("cooked_recipes");
            _logger.LogInformation($"Added CookedRecipe to Cache.");
            return cookedRecipeDTO;
        }
    }
}