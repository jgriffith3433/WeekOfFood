
namespace ContainerNinja.Contracts.Data.Entities
{
    public class CompletedOrder : AuditableEntity
    {
        public string Name { get; set; }
        public string? UserImport { get; set; }

        public virtual IList<CompletedOrderProduct> CompletedOrderProducts { get; private set; } = new List<CompletedOrderProduct>();
    }
}