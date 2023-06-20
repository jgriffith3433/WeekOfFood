
namespace ContainerNinja.Contracts.DTO
{
    public class OrderProductDTO
    {
        public int Id { get; set; }
        public long? WalmartId { get; set; }
        public string Name { get; set; }
        public ProductDTO? Product { get; set; }
    }
}