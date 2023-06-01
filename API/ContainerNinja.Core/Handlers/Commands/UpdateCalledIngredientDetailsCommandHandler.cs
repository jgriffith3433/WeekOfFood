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
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateCalledIngredientDetailsCommand : IRequest<CalledIngredientDetailsDTO>
    {
        public int Id { get; init; }

        public UnitType UnitType { get; init; }

        public int? ProductStockId { get; init; }

        public string? Name { get; init; }

        public float? Units { get; init; }
    }

    public class UpdateCalledIngredientDetailsCommandHandler : IRequestHandler<UpdateCalledIngredientDetailsCommand, CalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<UpdateCalledIngredientDetailsCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCalledIngredientDetailsCommandHandler> _logger;

        public UpdateCalledIngredientDetailsCommandHandler(ILogger<UpdateCalledIngredientDetailsCommandHandler> logger, IUnitOfWork repository, IValidator<UpdateCalledIngredientDetailsCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<CalledIngredientDetailsDTO> IRequestHandler<UpdateCalledIngredientDetailsCommand, CalledIngredientDetailsDTO>.Handle(UpdateCalledIngredientDetailsCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                throw new InvalidRequestBodyException
                {
                    Errors = errors
                };
            }

            var calledIngredientEntity = _repository.CalledIngredients
                .Include<CalledIngredient, ProductStock>(ci => ci.ProductStock)
                .Include<CalledIngredient, Recipe>(ci => ci.Recipe)
                .FirstOrDefault(co => co.Id == request.Id);

            if (calledIngredientEntity == null)
            {
                throw new EntityNotFoundException($"No CalledIngredient found for the Id {request.Id}");
            }

            calledIngredientEntity.Name = request.Name;
            calledIngredientEntity.Units = request.Units;
            calledIngredientEntity.UnitType = request.UnitType;

            if (request.ProductStockId.HasValue)
            {
                calledIngredientEntity.ProductStock = _repository.ProductStocks.Get(request.ProductStockId.Value);
            }

            _repository.CalledIngredients.Update(calledIngredientEntity);
            await _repository.CommitAsync();

            var calledIngredientDTO = _mapper.Map<CalledIngredientDTO>(calledIngredientEntity);

            _cache.SetItem($"called_ingredient_{request.Id}", calledIngredientDTO);
            _cache.RemoveItem("called_ingredients");
            _cache.RemoveItem($"recipe_{calledIngredientEntity.Recipe.Id}");
            _cache.RemoveItem("recipes");

            var calledIngredientDetailsDTO = _mapper.Map<CalledIngredientDetailsDTO>(calledIngredientEntity);
            return calledIngredientDetailsDTO;
        }
    }
}