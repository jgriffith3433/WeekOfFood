using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductStockCommand : IRequest<ProductStockDTO>
    {
        public int Id { get; init; }

        public float? Units { get; init; }
    }

    public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductStockDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<UpdateProductStockCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductStockCommandHandler> _logger;

        public UpdateProductStockCommandHandler(ILogger<UpdateProductStockCommandHandler> logger, IUnitOfWork repository, IValidator<UpdateProductStockCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductStockDTO> IRequestHandler<UpdateProductStockCommand, ProductStockDTO>.Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
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

            var productStockEntity = _repository.ProductStocks.Get(request.Id);

            if (productStockEntity == null)
            {
                throw new EntityNotFoundException($"No Product Stock found for the Id {request.Id}");
            }

            productStockEntity.Units = request.Units;

            _repository.ProductStocks.Update(productStockEntity);

            await _repository.CommitAsync();
            var response = _mapper.Map<ProductStockDTO>(productStockEntity);

            if (_cache.GetItem<ProductStockDTO>($"product_stock_{response.Id}") != null)
            {
                _logger.LogInformation($"Product Stock Exists in Cache. Setting new Product Stock for the same Key.");
                _cache.SetItem($"product_stock_{request.Id}", response);
            }
            _cache.RemoveItem("product_stocks");

            return response;
        }
    }
}