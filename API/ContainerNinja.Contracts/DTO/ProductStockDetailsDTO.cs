
namespace ContainerNinja.Contracts.DTO
{
    public class ProductStockDetailsDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public float Units { get; set; }

        public int? ProductId { get; set; }

        public ProductDTO Product { get; set; }

        public IList<ProductDTO> ProductSearchItems { get; set; } = new List<ProductDTO>();
    }
}