﻿using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductStockDetailsCommand : IRequest<ProductStockDetailsDTO>
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public int ProductId { get; init; }

        public float Units { get; init; }
    }

    public class UpdateProductStockDetailsCommandHandler : IRequestHandler<UpdateProductStockDetailsCommand, ProductStockDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductStockDetailsCommandHandler> _logger;

        public UpdateProductStockDetailsCommandHandler(ILogger<UpdateProductStockDetailsCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductStockDetailsDTO> IRequestHandler<UpdateProductStockDetailsCommand, ProductStockDetailsDTO>.Handle(UpdateProductStockDetailsCommand request, CancellationToken cancellationToken)
        {
            var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == request.Id);

            if (productStockEntity == null)
            {
                throw new NotFoundException($"No Product Stock found for the Id {request.Id}");
            }

            if (productStockEntity.WalmartProduct == null || request.ProductId != productStockEntity.WalmartProduct.Id)
            {
                //First search for product stocks that are already linked to the product
                var alreadyLinkedProductStock = _repository.ProductStocks.Set.Where(p => p.WalmartProduct.Id == request.ProductId).FirstOrDefault();
                if (alreadyLinkedProductStock != null)
                {
                    //we found an existing product stock, merge
                    var calledIngredients = _repository.CalledIngredients.Set.Where(ci => ci.ProductStock != null && ci.ProductStock.Id == productStockEntity.Id);
                    foreach (var calledIngredient in calledIngredients)
                    {
                        calledIngredient.ProductStock = alreadyLinkedProductStock;
                        _repository.CalledIngredients.Update(calledIngredient);
                    }
                    _repository.ProductStocks.Delete(productStockEntity.Id);
                    productStockEntity = alreadyLinkedProductStock;
                }
                else
                {
                    //no product stock found that was already linked to product, link this one
                    var productEntity = _repository.WalmartProducts.Get(request.ProductId);
                    if (productStockEntity == null)
                    {
                        throw new NotFoundException($"No Product Stock found for the Id {request.ProductId}");
                    }
                    productStockEntity.WalmartProduct = productEntity;
                }
            }

            productStockEntity.Units = request.Units;
            productStockEntity.Name = request.Name;

            _repository.ProductStocks.Update(productStockEntity);

            await _repository.CommitAsync();

            var productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
            _cache.SetItem($"product_stock_{request.Id}", productStockDTO);
            _cache.RemoveItem("product_stocks");

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productStockEntity.WalmartProduct);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            _cache.RemoveItem("products");

            return _mapper.Map<ProductStockDetailsDTO>(productStockDTO);
        }
    }
}