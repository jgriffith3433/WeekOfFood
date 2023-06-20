
namespace ContainerNinja.Contracts.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public IList<OrderProductDTO> OrderProducts { get; set; } = new List<OrderProductDTO>();
    }
}