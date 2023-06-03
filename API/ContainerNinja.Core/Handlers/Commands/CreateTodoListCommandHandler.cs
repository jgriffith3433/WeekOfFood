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
    public class CreateTodoListCommand : IRequest<TodoListDTO>
    {
        public CreateOrUpdateTodoListDTO Model { get; }
        public CreateTodoListCommand(CreateOrUpdateTodoListDTO model)
        {
            this.Model = model;
        }
    }

    public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, TodoListDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<CreateOrUpdateTodoListDTO> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTodoListCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateTodoListCommandHandler(ILogger<CreateTodoListCommandHandler> logger, IUnitOfWork repository, IValidator<CreateOrUpdateTodoListDTO> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<TodoListDTO> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
        {
            CreateOrUpdateTodoListDTO model = request.Model;

            var result = _validator.Validate(model);

            _logger.LogInformation($"Validation result: {result}");

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
                throw new InvalidRequestBodyException
                {
                    Errors = errors
                };
            }

            var entity = new TodoList
            {
                Title = model.Title,
                Color = model.Color
            };

            _repository.TodoLists.Add(entity);
            await _repository.CommitAsync();
            var todoListDTO = _mapper.Map<TodoListDTO>(entity);
            _cache.SetItem($"todo_list_{entity.Id}", todoListDTO);
            _cache.RemoveItem("todo_lists");
            return todoListDTO;
        }
    }
}