
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO
{
    public class ProductStockDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public float? Units { get; set; }

        public UnitType UnitType { get; set; }

        public int? ProductId { get; set; }

        public WalmartProductDTO Product { get; set; }
    }
}