using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class CookedRecipeCalledIngredient : AuditableEntity
    {
        public virtual CookedRecipe CookedRecipe { get; set; }
        public virtual CalledIngredient? CalledIngredient { get; set; }
        public virtual ProductStock? ProductStock { get; set; }
        public string Name { get; set; }
        public UnitType UnitType { get; set; }
        public float? Units { get; set; }
    }
}