using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateCompletedOrderProductCommand : IRequest<int>
    {
        public string? Name { get; init; }
        public int CompletedOrderId { get; init; }
    }

    public class CreateCompletedOrderProductCommandHandler : IRequestHandler<CreateCompletedOrderProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<CreateCompletedOrderProductCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCompletedOrderProductCommandHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IWalmartService _walmartService;

        public CreateCompletedOrderProductCommandHandler(ILogger<CreateCompletedOrderProductCommandHandler> logger, IUnitOfWork repository, IValidator<CreateCompletedOrderProductCommand> validator, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _walmartService = walmartService;
        }

        public async Task<int> Handle(CreateCompletedOrderProductCommand request, CancellationToken cancellationToken)
        {
            var completedOrderEntity = _repository.CompletedOrders.Include<CompletedOrder, IList<CompletedOrderProduct>>(co => co.CompletedOrderProducts).FirstOrDefault(co => co.Id == request.CompletedOrderId);

            if (completedOrderEntity == null)
            {
                throw new NotFoundException($"No CompletedOrder found for the Id {request.CompletedOrderId}");
            }

            var completedOrderProductEntity = new CompletedOrderProduct
            {
                Name = request.Name
            };

            _repository.CompletedOrderProducts.Add(completedOrderProductEntity);
            completedOrderEntity.CompletedOrderProducts.Add(completedOrderProductEntity);

            _repository.CompletedOrders.Update(completedOrderEntity);

            var searchResponse = _walmartService.Search(completedOrderProductEntity.Name);

            completedOrderProductEntity.WalmartSearchResponse = JsonConvert.SerializeObject(searchResponse);

            await _repository.CommitAsync();

            var completedOrderProductDTO = _mapper.Map<CompletedOrderProductDTO>(completedOrderProductEntity);
            _cache.SetItem($"completed_order_product_{completedOrderProductDTO.Id}", completedOrderProductDTO);
            _cache.RemoveItem("completed_orders");

            return completedOrderProductEntity.Id;
        }
    }
}