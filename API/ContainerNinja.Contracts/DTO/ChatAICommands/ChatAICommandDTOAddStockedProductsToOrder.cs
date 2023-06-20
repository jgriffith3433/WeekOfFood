using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_items_to_order", "Add stocked products to order")]
public record ChatAICommandDTOAddStockedProductsToOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new order")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("ID of the order to add stocked products to")]
    public int OrderId { get; set; }
    [Required]
    [Description("Stocked Products to order")]
    public List<ChatAICommandDTOAddStockedProductsToOrder_StockedProduct> StockedProducts { get; set; }
}

public record ChatAICommandDTOAddStockedProductsToOrder_StockedProduct
{
    [Required]
    [Description("Id of the stocked product")]
    public int StockedProductId { get; set; }
    [Required]
    [Description("How many should be ordered")]
    public long Quantity { get; set; }
}
