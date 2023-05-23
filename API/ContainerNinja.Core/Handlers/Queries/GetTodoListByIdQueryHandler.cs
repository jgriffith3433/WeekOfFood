using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetTodoListByIdQuery : IRequest<TodoListDTO>
    {
        public int TodoListId { get; }
        public GetTodoListByIdQuery(int id)
        {
            TodoListId = id;
        }
    }

    public class GetTodoListByIdQueryHandler : IRequestHandler<GetTodoListByIdQuery, TodoListDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<GetTodoListByIdQueryHandler> _logger;

        public GetTodoListByIdQueryHandler(ILogger<GetTodoListByIdQueryHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TodoListDTO> Handle(GetTodoListByIdQuery request, CancellationToken cancellationToken)
        {
            var cachedTodoList = _cache.GetItem<TodoListDTO>($"todo_list_{request.TodoListId}");
            if (cachedTodoList != null)
            {
                _logger.LogInformation($"TodoList Exists in Cache. Return CachedTodoList.");
                return cachedTodoList;
            }

            _logger.LogInformation($"TodoList doesn't exist in Cache.");

            var todoList = await Task.FromResult(_repository.TodoLists.Get(request.TodoListId));
            if (todoList == null)
            {
                throw new EntityNotFoundException($"No TodoList found for Id {request.TodoListId}");
            }

            var result = _mapper.Map<TodoListDTO>(todoList);

            _logger.LogInformation($"Add TodoList to Cache and return.");
            _cache.SetItem($"todo_list_{request.TodoListId}", result);
            return result;
        }
    }
}