
namespace ContainerNinja.Contracts.Data.Entities
{
    public class ProductStock : AuditableEntity
    {
        public string Name { get; set; }
        public float? Units { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}