
namespace ContainerNinja.Contracts.Data.Entities
{
    public class CookedRecipe : AuditableEntity
    {
        public Recipe Recipe { get; set; }

        public IList<CookedRecipeCalledIngredient> CookedRecipeCalledIngredients { get; private set; } = new List<CookedRecipeCalledIngredient>();
    }
}