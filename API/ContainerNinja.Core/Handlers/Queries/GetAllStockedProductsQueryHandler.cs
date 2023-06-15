using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using System.Text.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllProductStocksQuery : IRequest<GetAllProductStocksVM>
    {
    }

    public class GetAllProductStocksQueryHandlerHandler : IRequestHandler<GetAllProductStocksQuery, GetAllProductStocksVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllProductStocksQueryHandlerHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllProductStocksVM> Handle(GetAllProductStocksQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllProductStocksVM>("product_stocks");

            if (cachedEntities == null)
            {
                var entities = _repository.ProductStocks.Include<ProductStock, Product>(ps => ps.Product).AsEnumerable();
                var result = new GetAllProductStocksVM
                {
                    ProductStocks = _mapper.Map<List<ProductStockDTO>>(entities),
                    UnitTypes = Enum.GetValues(typeof(UnitType))
                    .Cast<UnitType>()
                    .Select(p => new UnitTypeDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),
                };

                _cache.SetItem("product_stocks", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}