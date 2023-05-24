using AutoMapper;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Core.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Item, ItemDTO>();

            CreateMap<TodoList, TodoListDTO>();

            CreateMap<Product, ProductDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType))
                .ForMember(d => d.ProductStockId, opt => opt.MapFrom(mapExpression: s => s.ProductStock != null ? s.ProductStock.Id : -1));

            CreateMap<CompletedOrder, CompletedOrderDTO>();

            CreateMap<CompletedOrderProduct, CompletedOrderProductDTO>();

            CreateMap<ProductStock, ProductStockDTO>()
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => (int)s.Product.Id));
        }
    }
}
