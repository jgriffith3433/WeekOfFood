using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateCompletedOrderCommand : IRequest<int>
    {
        public string Name { get; init; }
        public string? UserImport { get; init; }
    }

    public class CreateCompletedOrderCommandHandler : IRequestHandler<CreateCompletedOrderCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<CreateCompletedOrderCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCompletedOrderCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateCompletedOrderCommandHandler(ILogger<CreateCompletedOrderCommandHandler> logger, IUnitOfWork repository, IValidator<CreateCompletedOrderCommand> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateCompletedOrderCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);

            _logger.LogInformation($"CreateCompletedOrderCommand Validation result: {result}");

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                throw new InvalidRequestBodyException
                {
                    Errors = errors
                };
            }


            var completedOrderEntity = new CompletedOrder();
            completedOrderEntity.Name = request.Name;

            _repository.CompletedOrders.Add(completedOrderEntity);

            var imported = false;
            if (request.UserImport != null)
            {
                imported = true;
            }

            completedOrderEntity.UserImport = request.UserImport;

            await _repository.CommitAsync();

            if (imported)
            {
                //entity.AddDomainEvent(new CompletedOrderUserImportEvent(entity));
            }

            //await _repository.CommitAsync();

            var completedOrderDTO = _mapper.Map<CompletedOrderDTO>(completedOrderEntity);
            _cache.SetItem($"completed_order_{completedOrderDTO.Id}", completedOrderDTO);
            _cache.RemoveItem("completed_orders");
            _logger.LogInformation($"Added CompletedOrder to Cache.");
            return completedOrderDTO.Id;
        }
    }
}