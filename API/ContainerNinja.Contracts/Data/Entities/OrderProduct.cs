
namespace ContainerNinja.Contracts.Data.Entities
{
    public class OrderProduct : AuditableEntity
    {
        public string Name { get; set; }
        public long? WalmartId { get; set; }
        public virtual Product? Product { get; set; }
    }
}