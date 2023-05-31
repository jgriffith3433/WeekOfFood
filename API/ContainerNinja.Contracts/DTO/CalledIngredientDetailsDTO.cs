using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class CalledIngredientDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductStockDTO ProductStock { get; set; }
        public float? Units { get; set; }
        public int UnitType { get; set; }
        public int ProductStockId { get; set; }
        //public UnitType UnitType { get; set; }

        public IList<ProductStockDTO> ProductStockSearchItems { get; set; } = new List<ProductStockDTO>();
    }
}