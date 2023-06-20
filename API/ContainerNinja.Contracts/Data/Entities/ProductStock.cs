
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class ProductStock : AuditableEntity
    {
        public string Name { get; set; }
        public float Units { get; set; }
        public UnitType UnitType { get; set; }
        public virtual WalmartProduct? WalmartProduct { get; set; } = null;
    }
}