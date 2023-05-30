using MediatR;
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
    public record UpdateProductSizeCommand : IRequest<ProductDTO>
    {
        public int Id { get; init; }

        public float Size { get; init; }
    }

    public class UpdateProductSizeCommandHandler : IRequestHandler<UpdateProductSizeCommand, ProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductSizeCommandHandler> _logger;

        public UpdateProductSizeCommandHandler(ILogger<UpdateProductSizeCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductDTO> IRequestHandler<UpdateProductSizeCommand, ProductDTO>.Handle(UpdateProductSizeCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.Products.Get(request.Id);

            if (productEntity == null)
            {
                throw new EntityNotFoundException($"No Product found for the Id {request.Id}");
            }

            productEntity.Size = request.Size;
            _repository.Products.Update(productEntity);
            await _repository.CommitAsync();

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", productDTO);
            _cache.RemoveItem("products");

            return productDTO;
        }
    }
}