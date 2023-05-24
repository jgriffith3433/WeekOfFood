using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllProductsVM
    {
        public List<ProductDTO> Products { get; set; }
        public List<UnitTypeDTO> UnitTypes { get; set; }
    }
}