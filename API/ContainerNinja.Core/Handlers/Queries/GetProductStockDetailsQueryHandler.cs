using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using Alachisoft.NCache.Client;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetProductStockDetailsQuery : IRequest<ProductStockDetailsDTO>
    {
        public int Id { get; init; }
        public string Name { get; init; }
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
                var productStockEntity = _repository.ProductStocks.Include<ProductStock, Product>(ps => ps.Product).FirstOrDefault(ps => ps.Id == request.Id);

                if (productStockEntity == null)
                {
                    throw new NotFoundException($"No Product Stock found for the Id {request.Id}");
                }

                productStockDTO = _mapper.Map<ProductStockDTO>(productStockEntity);
                _cache.SetItem($"product_stock_{request.Id}", productStockDTO);
            }

            var productStockDetailsDTO = _mapper.Map<ProductStockDetailsDTO>(productStockDTO);

            var searchResults = from p in _repository.Products.Include<Product, ProductStock>(p => p.ProductStock) where EF.Functions.Like(p.Name, string.Format("%{0}%", request.Name)) || p.Id == productStockDTO.ProductId select p;

            productStockDetailsDTO.ProductSearchItems = _mapper.Map<IEnumerable<ProductDTO>>(searchResults).ToList();

            foreach (var productSearchItem in productStockDetailsDTO.ProductSearchItems)
            {
                if (productSearchItem.ProductStockId == request.Id)
                {
                    productSearchItem.Name += " ( Linked )";
                }
                else if (productSearchItem.ProductStockId != null)
                {
                    productSearchItem.Name += " ( Merge )";
                }
            }
            return productStockDetailsDTO;
        }
    }
}