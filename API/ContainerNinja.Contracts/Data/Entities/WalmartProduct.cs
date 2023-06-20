using ContainerNinja.Contracts.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class WalmartProduct : AuditableEntity
    {
        public string Name { get; set; }
        public long? WalmartId { get; set; }
        public string? WalmartLink { get; set; }
        public string? WalmartSize { get; set; }
        public string? WalmartItemResponse { get; set; }
        public string? WalmartSearchResponse { get; set; }
        public string? Error { get; set; }
        public float Size { get; set; }
        public float Price { get; set; }
        public bool Verified { get; set; }
        public UnitType UnitType { get; set; }
        public virtual IList<CompletedOrderWalmartProduct> CompletedOrderProducts { get; private set; } = new List<CompletedOrderWalmartProduct>();
        public virtual IList<ProductStock>? ProductStocks { get; set; } = new List<ProductStock>();
    }
}