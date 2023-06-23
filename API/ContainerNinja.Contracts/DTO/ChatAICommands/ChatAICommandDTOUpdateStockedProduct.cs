//using ContainerNinja.Contracts.Common;
//using ContainerNinja.Contracts.Enum;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;

//namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("update_kitchen_products", "Update existing kitchen products when taking stock of items in the kitchen")]
//public record ChatAICommandDTOUpdateStockedProducts : ChatAICommandArgumentsDTO
//{
//    [Required]
//    [Description("List of products the user is taking stock of")]
//    public List<ChatAICommandDTOUpdateStockedProducts_StockedProduct> StockedProducts { get; set; }
//}

//public record ChatAICommandDTOUpdateStockedProducts_StockedProduct
//{
//    [Description("Id of the stocked product if it exists in the system")]
//    public int? StockedProductId { get; set; }
//    [Required]
//    [Description("How many units do they have in stock")]
//    public float Units { get; set; }
//    [Required]
//    [Description("Units type for the stocked item")]
//    public UnitType KitchenUnitType { get; set; }
//}
