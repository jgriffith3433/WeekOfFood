using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("link_stocked_product_to_walmart_product", "Links a stocked product to a walmart product by ID")]
public record ChatAICommandDTOLinkStockedProductToWalmartProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new to do list")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("ID of the stocked product")]
    public int StockedProductId { get; set; }
    [Required]
    [Description("ID of the walmart product")]
    public long? WalmartProductId { get; set; }
}
