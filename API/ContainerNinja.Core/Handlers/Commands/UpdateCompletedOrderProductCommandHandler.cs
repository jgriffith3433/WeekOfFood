using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Services;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateCompletedOrderProductCommand : IRequest<CompletedOrderProductDTO>
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public long? WalmartId { get; init; }
    }

    public class UpdateCompletedOrderProductCommandHandler : IRequestHandler<UpdateCompletedOrderProductCommand, CompletedOrderProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<UpdateCompletedOrderProductCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCompletedOrderProductCommandHandler> _logger;
        private readonly IWalmartService _walmartService;

        public UpdateCompletedOrderProductCommandHandler(ILogger<UpdateCompletedOrderProductCommandHandler> logger, IUnitOfWork repository, IValidator<UpdateCompletedOrderProductCommand> validator, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _walmartService = walmartService;
        }

        async Task<CompletedOrderProductDTO> IRequestHandler<UpdateCompletedOrderProductCommand, CompletedOrderProductDTO>.Handle(UpdateCompletedOrderProductCommand request, CancellationToken cancellationToken)
        {
            var completedOrderProductEntity = _repository.CompletedOrderProducts.Include<CompletedOrderProduct, Product>(cop => cop.Product).FirstOrDefault(cop => cop.Id == request.Id);

            if (completedOrderProductEntity == null)
            {
                throw new NotFoundException($"No Completed Order Product found for the Id {request.Id}");
            }

            if (completedOrderProductEntity.Name != request.Name && completedOrderProductEntity.WalmartId == null)
            {
                //Still searching for product
                completedOrderProductEntity.Name = request.Name;
                var searchResponse = await _walmartService.Search(completedOrderProductEntity.Name);
                completedOrderProductEntity.WalmartSearchResponse = JsonConvert.SerializeObject(searchResponse);
            }
            //TODO: Look into this absurd if statement
            if ((completedOrderProductEntity.WalmartId == null && request.WalmartId != null) || (completedOrderProductEntity.WalmartId != null && completedOrderProductEntity.WalmartId != request.WalmartId || completedOrderProductEntity.Product == null))
            {
                //Found walmart id by searching and user manually set the product by selecting walmart id
                completedOrderProductEntity.WalmartId = request.WalmartId;
                if (completedOrderProductEntity.WalmartId != null)
                {
                    try
                    {
                        var itemResponse = await _walmartService.GetItem(completedOrderProductEntity.WalmartId);
                        var serializedItemResponse = JsonConvert.SerializeObject(itemResponse);
                        completedOrderProductEntity.WalmartItemResponse = serializedItemResponse;
                        completedOrderProductEntity.Name = itemResponse.name;

                        var productEntity = _repository.Products.Include<Product, ProductStock>(p => p.ProductStock).FirstOrDefault(p => p.WalmartId == itemResponse.itemId);

                        if (productEntity != null)
                        {
                            productEntity.ProductStock.Units += 1;
                            _repository.ProductStocks.Update(productEntity.ProductStock);
                        }
                        else
                        {
                            productEntity = new Product
                            {
                                Name = itemResponse.name,
                                WalmartId = itemResponse.itemId,
                            };

                            //always ensure a product stock record exists for each product
                            productEntity.ProductStock = new ProductStock
                            {
                                Name = itemResponse.name,
                                Units = 1,
                                Product = productEntity
                            };
                            _repository.ProductStocks.Add(productEntity.ProductStock);
                            _repository.Products.Add(productEntity);
                        }

                        await _repository.CommitAsync();
                        var productDTO = _mapper.Map<ProductDTO>(productEntity);
                        _cache.SetItem($"product_{productEntity.Id}", productDTO);
                        _cache.RemoveItem("products");

                        var productStockDTO = _mapper.Map<ProductStockDTO>(productEntity.ProductStock);
                        _cache.SetItem($"product_stock_{productEntity.ProductStock.Id}", productStockDTO);
                        _cache.RemoveItem("product_stocks");

                        //always update values from walmart to keep synced
                        productEntity.WalmartItemResponse = serializedItemResponse;
                        productEntity.Name = itemResponse.name;
                        productEntity.Price = itemResponse.salePrice;
                        productEntity.WalmartSize = itemResponse.size;
                        productEntity.WalmartLink = string.Format("https://walmart.com/ip/{0}/{1}", itemResponse.name, itemResponse.itemId);
                        completedOrderProductEntity.Product = productEntity;
                        _repository.CompletedOrderProducts.Update(completedOrderProductEntity);
                    }
                    catch (Exception ex)
                    {
                        completedOrderProductEntity.WalmartError = ex.Message;
                    }
                }
            }
            await _repository.CommitAsync();

            var completedOrderProductDTO = _mapper.Map<CompletedOrderProductDTO>(completedOrderProductEntity);
            _cache.SetItem($"completed_order_product_{request.Id}", completedOrderProductDTO);
            _cache.RemoveItem("completed_orders");
            _cache.RemoveItem("completed_order_products");

            return completedOrderProductDTO;
        }
    }
}