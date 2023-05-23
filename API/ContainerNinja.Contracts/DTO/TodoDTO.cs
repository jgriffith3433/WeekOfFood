using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class TodoListDTO : AuditableEntity
    {
        public string Title { get; set; }
        public string Color { get; set; }

        //public IList<TodoItemDto> Items { get; set; }
    }
}