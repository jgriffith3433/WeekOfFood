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
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IWalmartService _walmartService;

        public CreateProductCommandHandler(ILogger<CreateProductCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _walmartService = walmartService;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
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

            var searchResponse = await _walmartService.Search(request.Name);

            productEntity.WalmartSearchResponse = JsonConvert.SerializeObject(searchResponse);

            _repository.Products.Add(productEntity);

            await _repository.CommitAsync();

            var productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
            _cache.SetItem($"product_stock_{productStockDTO.Id}", productStockDTO);
            _cache.RemoveItem("product_stocks");

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{productDTO.Id}", productDTO);
            _cache.RemoveItem("products");
            return productDTO.Id;

        }
    }
}