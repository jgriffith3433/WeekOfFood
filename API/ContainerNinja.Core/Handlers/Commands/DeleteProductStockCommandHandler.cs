using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Exceptions;

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
            var productStockEntity = _repository.ProductStocks.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productStockEntity == null)
            {
                throw new NotFoundException($"No Product Stock found for the Id {request.Id}");
            }

            _cache.RemoveItem("products");
            _cache.RemoveItem($"product_{productStockEntity.Product.Id}");
            _repository.Products.Delete(productStockEntity.Product.Id);

            _cache.RemoveItem("product_stocks");
            _cache.RemoveItem($"product_stock_{request.Id}");
            _repository.ProductStocks.Delete(request.Id);

            await _repository.CommitAsync();

            return request.Id;
        }
    }
}