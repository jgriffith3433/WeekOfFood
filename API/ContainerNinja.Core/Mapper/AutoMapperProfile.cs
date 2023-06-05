using AutoMapper;
using ContainerNinja.Contracts.ChatAI;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Handlers.ChatCommands;
using Newtonsoft.Json;
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

            CreateMap<ConsumeChatCommand, ConsumeChatCommandEditRecipeName>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandEditRecipeName>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandGoToPage>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandGoToPage>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandOrder>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandOrder>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandNone>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandNone>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandDefault>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandDefault>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandAddCookedRecipeIngredient>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandAddCookedRecipeIngredient>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandAddRecipeIngredient>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandAddRecipeIngredient>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandCookedRecipeSubstituteIngredient>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandCookedRecipeSubstituteIngredient>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandCreateCookedRecipe>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandCreateCookedRecipe>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandCreateProduct>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandCreateProduct>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandCreateRecipe>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandCreateRecipe>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandDeleteCookedRecipeIngredient>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandDeleteCookedRecipeIngredient>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandDeleteProduct>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandDeleteProduct>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandDeleteRecipe>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandDeleteRecipe>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandDeleteRecipeIngredient>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandDeleteRecipeIngredient>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandEditCookedRecipeIngredientUnitType>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandEditCookedRecipeIngredientUnitType>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandEditProductUnitType>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandEditProductUnitType>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandEditRecipeIngredientUnitType>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandEditRecipeIngredientUnitType>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandRecipeSubstituteIngredient>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandRecipeSubstituteIngredient>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandDeleteCookedRecipe>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandDeleteCookedRecipe>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            CreateMap<ConsumeChatCommand, ConsumeChatCommandAddTodoList>()
                .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommandAddTodoList>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));

            //CreateMap < ConsumeChatCommand, ConsumeChatCommand$$$$$> ()
            //    .ForMember(x => x.Command, opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ChatAICommand$$$$$>(s.RawChatAICommand.Substring(s.RawChatAICommand.IndexOf('{'), s.RawChatAICommand.LastIndexOf('}') - s.RawChatAICommand.IndexOf('{') + 1))));


        }
    }
}
