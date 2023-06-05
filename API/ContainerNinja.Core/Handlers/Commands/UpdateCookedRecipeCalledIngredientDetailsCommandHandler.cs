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
    public record UpdateCookedRecipeCalledIngredientDetailsCommand : IRequest<CookedRecipeCalledIngredientDetailsDTO>
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public float? Units { get; init; }

        public int? ProductStockId { get; init; }
    }

    public class UpdateCookedRecipeCalledIngredientDetailsCommandHandler : IRequestHandler<UpdateCookedRecipeCalledIngredientDetailsCommand, CookedRecipeCalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<UpdateCookedRecipeCalledIngredientDetailsCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCookedRecipeCalledIngredientDetailsCommandHandler> _logger;

        public UpdateCookedRecipeCalledIngredientDetailsCommandHandler(ILogger<UpdateCookedRecipeCalledIngredientDetailsCommandHandler> logger, IUnitOfWork repository, IValidator<UpdateCookedRecipeCalledIngredientDetailsCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<CookedRecipeCalledIngredientDetailsDTO> IRequestHandler<UpdateCookedRecipeCalledIngredientDetailsCommand, CookedRecipeCalledIngredientDetailsDTO>.Handle(UpdateCookedRecipeCalledIngredientDetailsCommand request, CancellationToken cancellationToken)
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

            var cookedRecipeCalledIngredientDetailsDTO = _mapper.Map<CookedRecipeCalledIngredientDetailsDTO>(cookedRecipeCalledIngredientDTO);

            var query = from ps in _repository.ProductStocks.Include<ProductStock, Product>(ps => ps.Product)
                        where EF.Functions.Like(ps.Name, string.Format("%{0}%", cookedRecipeCalledIngredientDetailsDTO.Name))
                        select ps;

            cookedRecipeCalledIngredientDetailsDTO.ProductStockSearchItems = _mapper.Map<List<ProductStockDTO>>(query.ToList());

            return cookedRecipeCalledIngredientDetailsDTO;
        }
    }
}