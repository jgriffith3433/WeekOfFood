using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllWalmartProductsVM
    {
        public List<WalmartProductDTO> WalmartProducts { get; set; }
        public List<UnitTypeDTO> UnitTypes { get; set; }
    }
}