using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetTodoListsQuery : IRequest<IEnumerable<TodoListDTO>>
    {
    }

    public class GetTodoListsQueryHandler : IRequestHandler<GetTodoListsQuery, IEnumerable<TodoListDTO>>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetTodoListsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<TodoListDTO>> Handle(GetTodoListsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<IEnumerable<TodoListDTO>>("todo_lists");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.TodoLists.GetAll());
                var result = _mapper.Map<IEnumerable<TodoListDTO>>(entities);

                _cache.SetItem("todo_lists", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}