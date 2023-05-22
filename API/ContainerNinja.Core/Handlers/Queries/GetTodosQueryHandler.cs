using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetTodosQuery : IRequest<IEnumerable<TodoListDTO>>
    {
    }

    public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, IEnumerable<TodoListDTO>>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public GetTodosQueryHandler(IUnitOfWork repository, IMapper mapper, IDistributedCache cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<TodoListDTO>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
        {
            var cachedEntitiesString = await _cache.GetStringAsync("all_todo_lists");

            if (cachedEntitiesString == null)
            {
                var entities = await Task.FromResult(_repository.TodoList.GetAll());
                var result = _mapper.Map<IEnumerable<TodoListDTO>>(entities);

                await _cache.SetStringAsync("all_todo_lists", JsonConvert.SerializeObject(result));
                return result;
            }
            else
            {
                var cachedEntities = JsonConvert.DeserializeObject<IEnumerable<TodoListDTO>>(cachedEntitiesString);
                return cachedEntities;
            }
        }
    }
}