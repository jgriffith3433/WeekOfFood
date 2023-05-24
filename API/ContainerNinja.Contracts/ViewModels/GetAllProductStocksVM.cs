using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllProductStocksVM
    {
        public List<ProductStockDTO> ProductStocks { get; set; }
        public List<UnitTypeDTO> UnitTypes { get; set; }
    }
}