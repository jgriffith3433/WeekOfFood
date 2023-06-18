using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("update_product_stock", "Update inventory of food goods")]
public record ChatAICommandDTOUpdateProductStock : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to take inventory")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("List of products the user is taking stock of")]
    public List<ChatAICommandDTOTakeStock_StockedProduct> StockedProducts { get; set; }
}

public record ChatAICommandDTOTakeStock_StockedProduct
{
    [Description("Id of the stocked product")]
    public int? StockedProductId { get; set; }
    [Required]
    [Description("Name of the stocked product")]
    public string StockedProductName { get; set; }
    [Required]
    [Description("How many units do they have")]
    public float? Quantity { get; set; }
    [Required]
    [Description("Type of unit")]
    public string? UnitType { get; set; }
}
