using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("link_stocked_products_to_walmart_products", "Links multiple stocked products to a walmart products by ID")]
public record ChatAICommandDTOLinkStockedProductsToWalmartProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new to do list")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("List of links to make")]
    public List<ChatAICommandDTOLinkStockedProductsToWalmartProducts_Link> Links { get; set; }
}

public record ChatAICommandDTOLinkStockedProductsToWalmartProducts_Link
{
    [Required]
    [Description("ID of the stocked product")]
    public int StockedProductId { get; set; }
    [Required]
    [Description("ID of the walmart product")]
    public long WalmartProductId { get; set; }
}