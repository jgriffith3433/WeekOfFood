using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class CalledIngredient : AuditableEntity
    {
        public string Name { get; set; }

        public float? Units { get; set; }

        public bool Verified { get; set; }

        public UnitType UnitType { get; set; }

        public ProductStock? ProductStock { get; set; } = null!;

        //public Recipe Recipe { get; set; } = null!;
    }
}