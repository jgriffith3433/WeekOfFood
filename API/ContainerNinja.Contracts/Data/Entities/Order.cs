
namespace ContainerNinja.Contracts.Data.Entities
{
    public class Order : AuditableEntity
    {
        public virtual IList<OrderProduct> OrderProducts { get; private set; } = new List<OrderProduct>();
    }
}