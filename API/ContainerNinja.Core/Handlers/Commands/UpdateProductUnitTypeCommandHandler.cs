using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductUnitTypeCommand : IRequest<ProductDTO>
    {
        public int Id { get; init; }

        public int UnitType { get; init; }
    }

    public class UpdateProductUnitTypeCommandHandler : IRequestHandler<UpdateProductUnitTypeCommand, ProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductUnitTypeCommandHandler> _logger;

        public UpdateProductUnitTypeCommandHandler(ILogger<UpdateProductUnitTypeCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductDTO> IRequestHandler<UpdateProductUnitTypeCommand, ProductDTO>.Handle(UpdateProductUnitTypeCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.Products.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new NotFoundException($"No Product found for the Id {request.Id}");
            }

            productEntity.UnitType = (UnitType)request.UnitType;
            _repository.Products.Update(productEntity);
            await _repository.CommitAsync();

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