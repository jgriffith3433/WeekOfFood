using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_walmart_products_for_kitchen_product" }, "Searches for a walmart products for an unlinked kitchen product")]
public record ChatAICommandDTOSearchWalmartProductsForKitchenProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("IDs of a Kitchen Products")]
    public int[] KitchenProductIds { get; set; }
}