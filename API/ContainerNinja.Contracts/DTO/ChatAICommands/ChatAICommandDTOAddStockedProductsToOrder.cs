using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_kitchen_products_to_order", "Add kitch products to order")]
public record ChatAICommandDTOAddStockedProductsToOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the order to add stocked products to")]
    public int OrderId { get; set; }
    [Required]
    [Description("Stocked Products to order")]
    public List<ChatAICommandDTOAddStockedProductsToOrder_StockedProduct> StockedProducts { get; set; }
}

public record ChatAICommandDTOAddStockedProductsToOrder_StockedProduct
{
    [Required]
    [Description("Id of the kitchen product")]
    public int KitchenProductId { get; set; }
    [Required]
    [Description("How many should be ordered. Convert from the stocked product unit type to the walmart size to get this number.")]
    public int OrderQuantity { get; set; }
}
