using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_walmart_products_for_stocked_product" }, "Searches for a walmart products for an unlinked stocked product")]
public record ChatAICommandDTOSearchWalmartProductsForStockedProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("IDs of a Stocked Products")]
    public int[] StockedProductIds { get; set; }
}