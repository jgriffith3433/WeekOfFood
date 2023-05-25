using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteProductStockCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteProductStockCommandHandler : IRequestHandler<DeleteProductStockCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteProductStockCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteProductStockCommand request, CancellationToken cancellationToken)
        {
            _repository.ProductStocks.Delete(request.Id);
            await _repository.CommitAsync();
            _cache.RemoveItem("product_stocks");
            _cache.RemoveItem($"product_stock_{request.Id}");
            return request.Id;
        }
    }
}