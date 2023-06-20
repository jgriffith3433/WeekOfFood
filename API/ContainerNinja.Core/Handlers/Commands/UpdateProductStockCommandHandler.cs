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
    public record UpdateProductStockCommand : IRequest<ProductStockDTO>
    {
        public int Id { get; init; }

        public float Units { get; init; }
    }

    public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductStockDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductStockCommandHandler> _logger;

        public UpdateProductStockCommandHandler(ILogger<UpdateProductStockCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductStockDTO> IRequestHandler<UpdateProductStockCommand, ProductStockDTO>.Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
        {
            var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(ps => ps.Id == request.Id);

            if (productStockEntity == null)
            {
                throw new NotFoundException($"No Product Stock found for the Id {request.Id}");
            }

            productStockEntity.Units = request.Units;

            _repository.ProductStocks.Update(productStockEntity);

            await _repository.CommitAsync();

            var productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
            _cache.Clear();
            _cache.SetItem($"product_stock_{request.Id}", productStockDTO);

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productStockEntity.WalmartProduct);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            return productStockDTO;
        }
    }
}