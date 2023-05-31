using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class CalledIngredientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductStockDTO ProductStock { get; set; }
        public float? Units { get; set; }
        public int UnitType { get; set; }
    }
}