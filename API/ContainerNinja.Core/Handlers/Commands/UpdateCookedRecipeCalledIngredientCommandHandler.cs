using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateCookedRecipeCalledIngredientCommand : IRequest<CookedRecipeCalledIngredientDTO>
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public float? Units { get; init; }

        public int? ProductStockId { get; init; }
    }

    public class UpdateCookedRecipeCalledIngredientCommandHandler : IRequestHandler<UpdateCookedRecipeCalledIngredientCommand, CookedRecipeCalledIngredientDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCookedRecipeCalledIngredientCommandHandler> _logger;

        public UpdateCookedRecipeCalledIngredientCommandHandler(ILogger<UpdateCookedRecipeCalledIngredientCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<CookedRecipeCalledIngredientDTO> IRequestHandler<UpdateCookedRecipeCalledIngredientCommand, CookedRecipeCalledIngredientDTO>.Handle(UpdateCookedRecipeCalledIngredientCommand request, CancellationToken cancellationToken)
        {
            var cookedRecipeCalledIngredientEntity = _repository.CookedRecipeCalledIngredients.Include<CookedRecipeCalledIngredient, CalledIngredient>(crci => crci.CalledIngredient).Include(crci => crci.ProductStock).FirstOrDefault(co => co.Id == request.Id);

            if (cookedRecipeCalledIngredientEntity == null)
            {
                throw new NotFoundException($"No CookedRecipeCalledIngredient found for the Id {request.Id}");
            }

            cookedRecipeCalledIngredientEntity.Name = request.Name;
            cookedRecipeCalledIngredientEntity.Units = request.Units;

            if (request.ProductStockId.HasValue)
            {
                var productStock = _repository.ProductStocks.Get(request.ProductStockId.Value);

                if (productStock == null)
                {
                    throw new NotFoundException($"No ProductStock found for the Id {request.ProductStockId}");
                }
                cookedRecipeCalledIngredientEntity.ProductStock = productStock;
            }
            _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredientEntity);
            await _repository.CommitAsync();

            var cookedRecipeCalledIngredientDTO = _mapper.Map<CookedRecipeCalledIngredientDTO>(cookedRecipeCalledIngredientEntity);
            _cache.SetItem($"cooked_recipe_called_ingredient_{request.Id}", cookedRecipeCalledIngredientDTO);
            _cache.RemoveItem("cooked_recipe_called_ingredients");

            return cookedRecipeCalledIngredientDTO;
        }
    }
}