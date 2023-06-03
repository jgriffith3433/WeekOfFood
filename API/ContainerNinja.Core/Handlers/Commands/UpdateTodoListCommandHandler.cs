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
    public class UpdateTodoListCommand : IRequest<TodoListDTO>
    {
        public int TodoListId { get; set; }
        public CreateOrUpdateTodoListDTO Model { get; }

        public UpdateTodoListCommand(int todoListId, CreateOrUpdateTodoListDTO model)
        {
            this.TodoListId = todoListId;
            this.Model = model;
        }
    }

    public class UpdateTodoListCommandHandler : IRequestHandler<UpdateTodoListCommand, TodoListDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IValidator<CreateOrUpdateTodoListDTO> _validator;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateTodoListCommandHandler> _logger;

        public UpdateTodoListCommandHandler(ILogger<UpdateTodoListCommandHandler> logger, IUnitOfWork repository, IValidator<CreateOrUpdateTodoListDTO> validator, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<TodoListDTO> IRequestHandler<UpdateTodoListCommand, TodoListDTO>.Handle(UpdateTodoListCommand request, CancellationToken cancellationToken)
        {
            CreateOrUpdateTodoListDTO model = request.Model;
            int todoListId = request.TodoListId;

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

            var dbEntity = _repository.TodoLists.Get(todoListId);

            if (dbEntity == null)
            {
                throw new EntityNotFoundException($"No TodoList found for the Id {todoListId}");
            }

            dbEntity.Title = model.Title;
            dbEntity.Color = model.Color;

            _repository.TodoLists.Update(dbEntity);
            await _repository.CommitAsync();

            var updatedTodoList = _mapper.Map<TodoListDTO>(dbEntity);
            _cache.SetItem($"todo_list_{todoListId}", updatedTodoList);
            _cache.RemoveItem("todo_lists");
            return updatedTodoList;
        }
    }
}