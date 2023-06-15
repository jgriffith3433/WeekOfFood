using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class CalledIngredient : AuditableEntity
    {
        public string Name { get; set; }

        public float? Units { get; set; }

        public bool Verified { get; set; }

        public UnitType UnitType { get; set; }

        public virtual ProductStock? ProductStock { get; set; } = null!;

        public virtual Recipe Recipe { get; set; }

        public virtual IList<CookedRecipeCalledIngredient> CookedRecipeCalledIngredients { get; private set; } = new List<CookedRecipeCalledIngredient>();
    }
}