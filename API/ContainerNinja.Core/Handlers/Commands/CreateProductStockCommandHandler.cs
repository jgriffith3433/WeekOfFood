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
        private readonly IValidator<CreateProductStockCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductStockCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateProductStockCommandHandler(ILogger<CreateProductStockCommandHandler> logger, IUnitOfWork repository, IValidator<CreateProductStockCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateProductStockCommand request, CancellationToken cancellationToken)
        {
            var productStockEntity = new ProductStock
            {
                Name = request.Name
            };

            var productEntity = new Product
            {
                Name = request.Name,
                ProductStock = productStockEntity
            };

            _repository.ProductStocks.Add(productStockEntity);
            _repository.Products.Add(productEntity);

            await _repository.CommitAsync();

            var productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
            _cache.SetItem($"product_stock_{productStockDTO.Id}", productStockDTO);
            _cache.RemoveItem("product_stocks");

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{productDTO.Id}", productDTO);
            _cache.RemoveItem("products");
            return productStockDTO.Id;
        }
    }
}