using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateProductStockCommand : IRequest<int>
    {
        public string Name { get; init; }
    }

    public class CreateProductStockCommandHandler : IRequestHandler<CreateProductStockCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductStockCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateProductStockCommandHandler(ILogger<CreateProductStockCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateProductStockCommand request, CancellationToken cancellationToken)
        {
            var existingProductStockWithName = _repository.ProductStocks.Set.FirstOrDefault(p => p.Name.ToLower() == request.Name.ToLower());

            if (existingProductStockWithName != null)
            {
                var systemResponse = "ProductStock already exists: " + request.Name;
                throw new Exception(systemResponse);
            }

            var productStockEntity = _repository.ProductStocks.CreateProxy();
            {
                productStockEntity.Name = request.Name;
            };

            _repository.ProductStocks.Add(productStockEntity);

            await _repository.CommitAsync();

            var productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
            _cache.SetItem($"product_stock_{productStockDTO.Id}", productStockDTO);
            _cache.RemoveItem("product_stocks");
            _cache.RemoveItem("products");
            return productStockDTO.Id;
        }
    }
}