using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetProductDetailsQuery : IRequest<ProductDetailsDTO>
    {
        public int Id { get; init; }
    }

    public class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, ProductDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetProductDetailsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ProductDetailsDTO> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            var productDTO = _cache.GetItem<ProductDTO>($"product_{request.Id}");
            if (productDTO == null)
            {
                var productEntity = _repository.Products.Get(request.Id);

                if (productEntity == null)
                {
                    throw new NotFoundException($"No Product found for the Id {request.Id}");
                }

                productDTO = _mapper.Map<ProductDTO>(productEntity);
                _cache.SetItem($"product_{request.Id}", productDTO);
            }

            var productDetailsDTO = _mapper.Map<ProductDetailsDTO>(productDTO);

            return productDetailsDTO;
        }
    }
}