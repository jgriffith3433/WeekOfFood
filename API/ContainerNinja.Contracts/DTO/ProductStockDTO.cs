
namespace ContainerNinja.Contracts.DTO
{
    public class ProductStockDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public float? Units { get; set; }

        public int? ProductId { get; set; }

        public ProductDTO Product { get; set; }
    }
}