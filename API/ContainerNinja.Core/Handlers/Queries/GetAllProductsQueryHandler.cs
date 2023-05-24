using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllProductsQuery : IRequest<GetAllProductsVM>
    {
    }

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, GetAllProductsVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllProductsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllProductsVM> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllProductsVM>("products");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.Products.GetAll());
                var result = new GetAllProductsVM
                {
                    Products = _mapper.Map<List<ProductDTO>>(entities),
                    UnitTypes = Enum.GetValues(typeof(UnitType))
                    .Cast<UnitType>()
                    .Select(p => new UnitTypeDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),
                };

                _cache.SetItem("products", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }

        }
    }
}