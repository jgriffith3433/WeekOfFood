using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO
{
    public class CookedRecipeCalledIngredientDetailsDTO
    {
        public int Id { get; set; }
        public int CookedRecipeId { get; set; }
        public CalledIngredientDTO? CalledIngredient { get; set; }
        public ProductStockDTO? ProductStock { get; set; }
        public int? ProductStockId { get; set; }
        public string Name { get; set; }
        public UnitType UnitType { get; set; }
        public float? Units { get; set; }

        public List<ProductStockDTO> ProductStockSearchItems { get; set; }
    }
}