﻿using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductNameCommand : IRequest<ProductDTO>
    {
        public int Id { get; init; }

        public string Name { get; init; }
    }

    public class UpdateProductNameCommandHandler : IRequestHandler<UpdateProductNameCommand, ProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<UpdateProductNameCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductNameCommandHandler> _logger;

        public UpdateProductNameCommandHandler(ILogger<UpdateProductNameCommandHandler> logger, IUnitOfWork repository, IValidator<UpdateProductNameCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductDTO> IRequestHandler<UpdateProductNameCommand, ProductDTO>.Handle(UpdateProductNameCommand request, CancellationToken cancellationToken)
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

            var productEntity = _repository.Products.Include<Product, ProductStock>(p => p.ProductStock).FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new EntityNotFoundException($"No Product found for the Id {request.Id}");
            }

            if (productEntity.Name != request.Name && productEntity.WalmartId == null)
            {
                //Still searching for walmart product
                productEntity.Name = request.Name;
                productEntity.ProductStock.Name = request.Name;

                //var searchResponse = _walmartApiService.Search(request.Name);

                //productEntity.WalmartSearchResponse = JsonConvert.SerializeObject(searchResponse);
                _repository.Products.Update(productEntity);
                _repository.ProductStocks.Update(productEntity.ProductStock);
                await _repository.CommitAsync();
            }

            var productStockDTO = _mapper.Map<ProductStockDTO>(productEntity.ProductStock);
            _cache.SetItem($"product_stock_{request.Id}", productStockDTO);
            _cache.RemoveItem("product_stocks");

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", productDTO);
            _cache.RemoveItem("products");

            return productDTO;
        }
    }
}