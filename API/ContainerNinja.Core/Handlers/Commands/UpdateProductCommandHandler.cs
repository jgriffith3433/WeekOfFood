using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductCommand : IRequest<ProductDTO>
    {
        public int Id { get; init; }

        public long? WalmartId { get; init; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<UpdateProductCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(ILogger<UpdateProductCommandHandler> logger, IUnitOfWork repository, IValidator<UpdateProductCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ProductDTO> IRequestHandler<UpdateProductCommand, ProductDTO>.Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                throw new InvalidRequestBodyException
                {
                    Errors = errors
                };
            }

            var productEntity = _repository.Products.Get(request.Id);

            if (productEntity == null)
            {
                throw new EntityNotFoundException($"No Product found for the Id {request.Id}");
            }

            if (productEntity.WalmartId != request.WalmartId && request.WalmartId != null)
            {
                //Found walmart id by searching and user manually set the product by selecting walmart id

                var existingProductEntity = _repository.Products.GetAll().Where(p => p.WalmartId == request.WalmartId).FirstOrDefault();

                if (existingProductEntity != null)
                {
                    throw new EntityNotFoundException($"Product already exists with that walmart id {request.WalmartId}");
                }

                productEntity.WalmartId = request.WalmartId;

                try
                {
                    //var itemResponse = _walmartApiService.GetItem(request.WalmartId);

                    //var serializedItemResponse = JsonConvert.SerializeObject(itemResponse);

                    ////always update values from walmart to keep synced
                    //productEntity.WalmartItemResponse = serializedItemResponse;
                    //productEntity.Name = itemResponse.name;
                    //productEntity.ProductStock.Name = itemResponse.name;
                    //productEntity.Price = itemResponse.salePrice;
                    //productEntity.WalmartSize = itemResponse.size;
                    //productEntity.WalmartLink = string.Format("https://walmart.com/ip/{0}/{1}", itemResponse.name, itemResponse.itemId);
                }
                catch (Exception ex)
                {
                    productEntity.Error = ex.Message;
                }
                _repository.Products.Update(productEntity);
                await _repository.CommitAsync();
            }

            var productDTO = _mapper.Map<ProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", productDTO);
            _cache.RemoveItem("products");

            return productDTO;
        }
    }
}