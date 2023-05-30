﻿using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Enum;

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
            var productEntity = _repository.Products.Get(request.Id);

            if (productEntity == null)
            {
                throw new EntityNotFoundException($"No Product found for the Id {request.Id}");
            }

            productEntity.UnitType = (UnitType)request.UnitType;
            _repository.Products.Update(productEntity);
            await _repository.CommitAsync();

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", productDTO);
            _cache.RemoveItem("products");

            return productDTO;
        }
    }
}