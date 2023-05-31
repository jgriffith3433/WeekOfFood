using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class SearchCalledIngredientProductStockNameQuery : IRequest<CalledIngredientDetailsDTO>
    {
        public int Id { get; init; }
        public string Search { get; init; }
    }

    public class SearchCalledIngredientProductNameQueryHandler : IRequestHandler<SearchCalledIngredientProductStockNameQuery, CalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public SearchCalledIngredientProductNameQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CalledIngredientDetailsDTO> Handle(SearchCalledIngredientProductStockNameQuery request, CancellationToken cancellationToken)
        {
            var calledIngredientDTO = _cache.GetItem<CalledIngredientDTO>($"called_ingredient_{request.Id}");
            if (calledIngredientDTO == null)
            {
                var calledIngredientEntity = _repository.CalledIngredients.Include<CalledIngredient, ProductStock>(co => co.ProductStock).FirstOrDefault(co => co.Id == request.Id);

                if (calledIngredientEntity == null)
                {
                    throw new EntityNotFoundException($"No CalledIngredient found for the Id {request.Id}");
                }

                calledIngredientDTO = _mapper.Map<CalledIngredientDTO>(calledIngredientEntity);
                _cache.SetItem($"called_ingredient_{request.Id}", calledIngredientDTO);
            }

            var calledIngredientDetailsDTO = _mapper.Map<CalledIngredientDetailsDTO>(calledIngredientDTO);

            var query = from ps in _repository.ProductStocks.Include<ProductStock, Product>(ps => ps.Product)
                        where EF.Functions.Like(ps.Name, string.Format("%{0}%", request.Search))
                        select ps;

            calledIngredientDetailsDTO.ProductStockSearchItems = _mapper.Map<List<ProductStockDTO>>(query.ToList());

            return calledIngredientDetailsDTO;
        }
    }
}