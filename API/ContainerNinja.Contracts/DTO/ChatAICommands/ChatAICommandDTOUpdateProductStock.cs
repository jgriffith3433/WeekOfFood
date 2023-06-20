using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_stock_products", "Create records of stocked products and update existing stocked product's units when taking stock of items in the kitchen")]
public record ChatAICommandDTOUpdateStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to take inventory")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("List of products the user is taking stock of")]
    public List<ChatAICommandDTOUpdateStockedProducts_StockedProduct> StockedProducts { get; set; }
}

public record ChatAICommandDTOUpdateStockedProducts_StockedProduct
{
    [Description("Id of the stocked product if it exists in the system")]
    public int? StockedProductId { get; set; }
    [Required]
    [Description("Name of the stocked product")]
    public string StockedProductName { get; set; }
    [Required]
    [Description("How many units do they have in stock")]
    public float Units { get; set; }
    [Required]
    [Description("Units type for the stocked item")]
    public UnitType UnitType { get; set; }
}
