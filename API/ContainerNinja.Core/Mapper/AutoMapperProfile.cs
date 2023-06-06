using AutoMapper;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.RequestModels;

namespace ContainerNinja.Core.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Item, ItemDTO>();

            CreateMap<TodoList, TodoListDTO>();

            CreateMap<TodoItem, TodoItemDTO>()
                .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));

            CreateMap<Product, ProductDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType))
                .ForMember(d => d.ProductStockId, opt => opt.MapFrom(mapExpression: s => s.ProductStock != null ? s.ProductStock.Id : -1));

            CreateMap<Product, ProductDetailsDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType))
                .ForMember(d => d.ProductStockId, opt => opt.MapFrom(mapExpression: s => s.ProductStock != null ? s.ProductStock.Id : -1));

            CreateMap<ProductDTO, ProductDetailsDTO>();
            CreateMap<ProductDetailsDTO, ProductDTO>();

            CreateMap<CompletedOrder, CompletedOrderDTO>();

            CreateMap<CompletedOrderProduct, CompletedOrderProductDTO>();

            CreateMap<ProductStock, ProductStockDTO>()
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => (int)s.Product.Id));

            CreateMap<ProductStock, ProductStockDetailsDTO>()
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => (int)s.Product.Id));

            CreateMap<ProductStockDTO, ProductStockDetailsDTO>();
            CreateMap<ProductStockDetailsDTO, ProductStockDTO>();

            CreateMap<Recipe, RecipeDTO>();

            CreateMap<CalledIngredient, CalledIngredientDetailsDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType))
                .ForMember(d => d.ProductStockId, opt => opt.MapFrom(mapExpression: s => s.ProductStock != null ? s.ProductStock.Id : -1));

            CreateMap<CalledIngredient, CalledIngredientDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType));

            CreateMap<CalledIngredientDTO, CalledIngredientDetailsDTO>();
            CreateMap<CalledIngredientDetailsDTO, CalledIngredientDTO>();

            CreateMap<CookedRecipe, CookedRecipeDTO>()
                .ForMember(d => d.RecipeId, opt => opt.MapFrom(mapExpression: s => s.Recipe != null ? s.Recipe.Id : -1));

            CreateMap<CookedRecipeCalledIngredient, CookedRecipeCalledIngredientDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType))
                .ForMember(d => d.CookedRecipeId, opt => opt.MapFrom(s => s.CookedRecipe.Id))
                .ForMember(d => d.ProductStockId, opt => opt.MapFrom(mapExpression: s => (int?)(s.ProductStock != null ? s.ProductStock.Id : null)));

            CreateMap<CookedRecipeCalledIngredient, CookedRecipeCalledIngredientDetailsDTO>()
                .ForMember(d => d.UnitType, opt => opt.MapFrom(s => (int)s.UnitType))
                .ForMember(d => d.CookedRecipeId, opt => opt.MapFrom(s => s.CookedRecipe.Id))
                .ForMember(d => d.ProductStockId, opt => opt.MapFrom(mapExpression: s => (int?)(s.ProductStock != null ? s.ProductStock.Id : null)));

            CreateMap<CookedRecipeCalledIngredientDTO, CookedRecipeCalledIngredientDetailsDTO>();
            CreateMap<CookedRecipeCalledIngredientDetailsDTO, CookedRecipeCalledIngredientDTO>();

            CreateMap<ChatMessage, ChatMessageVM>();
            CreateMap<ChatMessageVM, ChatMessage>();

        }
    }
}
