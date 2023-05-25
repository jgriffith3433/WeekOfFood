using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetProductStockDetailsQuery : IRequest<ProductStockDetailsDTO>
    {
        public int Id { get; init; }
        public string Search { get; init; }
    }

    public class GetProductStockDetailsQueryHandler : IRequestHandler<GetProductStockDetailsQuery, ProductStockDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetProductStockDetailsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ProductStockDetailsDTO> Handle(GetProductStockDetailsQuery request, CancellationToken cancellationToken)
        {
            var productStockDTO = _cache.GetItem<ProductStockDTO>($"product_stock_{request.Id}");
            if (productStockDTO == null)
            {
                var productStockEntity = _repository.ProductStocks.Get(request.Id);

                if (productStockEntity == null)
                {
                    throw new EntityNotFoundException($"No Product Stock found for the Id {request.Id}");
                }

                productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
                _cache.SetItem($"product_stock_{request.Id}", productStockDTO);
            }

            var productStockDetailsDTO = _mapper.Map<ProductStockDetailsDTO>(productStockDTO);

            var searchResults = _repository.Products.SearchForByName(request.Search);

            productStockDetailsDTO.ProductSearchItems = _mapper.Map<IEnumerable<ProductDTO>>(searchResults).ToList();

            foreach (var productSearchItem in productStockDetailsDTO.ProductSearchItems)
            {
                if (productSearchItem.ProductStockId != null)
                {
                    productSearchItem.Name += " ( Merge )";
                }
            }
            return productStockDetailsDTO;
        }
    }
}