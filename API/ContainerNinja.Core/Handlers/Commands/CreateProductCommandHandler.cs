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

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateProductCommand : IRequest<int>
    {
        public string Name { get; init; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<CreateProductCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateProductCommandHandler(ILogger<CreateProductCommandHandler> logger, IUnitOfWork repository, IValidator<CreateProductCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);

            _logger.LogInformation($"CreateProductCommand Validation result: {result}");

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                throw new InvalidRequestBodyException
                {
                    Errors = errors
                };
            }


            var productEntity = new Product
            {
                Name = request.Name
            };

            //always ensure a product stock record exists for each product
            var productStockEntity = new ProductStock
            {
                Name = request.Name,
                Units = 1
            };
            _repository.ProductStocks.Add(productStockEntity);

            productStockEntity.Product = productEntity;

            //var searchResponse = _walmartApiService.Search(request.Name);

            //entity.WalmartSearchResponse = JsonConvert.SerializeObject(searchResponse);

            _repository.Products.Add(productEntity);

            await _repository.CommitAsync();

            var productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
            _cache.SetItem($"product_stock_{productStockDTO.Id}", productStockDTO);
            _cache.RemoveItem("product_stocks");
            _logger.LogInformation($"Added ProductStock to Cache.");

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{productDTO.Id}", productDTO);
            _cache.RemoveItem("products");
            _logger.LogInformation($"Added Product to Cache.");
            return productDTO.Id;

        }
    }
}