using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class SearchCompletedOrderProductNameQuery : IRequest<CompletedOrderProductDTO>
    {
        public int Id { get; init; }
    }

    public class SearchCompletedOrderProductNameQueryHandler : IRequestHandler<SearchCompletedOrderProductNameQuery, CompletedOrderProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public SearchCompletedOrderProductNameQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CompletedOrderProductDTO> Handle(SearchCompletedOrderProductNameQuery request, CancellationToken cancellationToken)
        {
            var completedOrderProductDTO = _cache.GetItem<CompletedOrderProductDTO>($"completed_order_product_{request.Id}");
            if (completedOrderProductDTO == null)
            {
                var completedOrderProductEntity = _repository.CompletedOrderProducts.Include<CompletedOrderProduct, CompletedOrder>(cop => cop.CompletedOrder).FirstOrDefault(cop => cop.Id == request.Id);

                if (completedOrderProductEntity == null)
                {
                    throw new EntityNotFoundException($"No CompletedOrderProduct found for the Id {request.Id}");
                }

                completedOrderProductDTO = _mapper.Map<CompletedOrderProductDTO>(completedOrderProductEntity);
                _cache.SetItem($"completed_order_product_{request.Id}", completedOrderProductDTO);
            }

            //var searchResponse = _walmartApiService.Search(request.Search);

            //completedOrderProductDTO.WalmartSearchResponse = JsonConvert.SerializeObject(searchResponse);
            await _repository.CommitAsync();

            return completedOrderProductDTO;
        }
    }
}