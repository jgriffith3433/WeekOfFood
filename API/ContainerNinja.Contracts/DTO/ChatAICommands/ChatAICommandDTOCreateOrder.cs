using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_order", "Creates a new order")]
public record ChatAICommandDTOCreateOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new order")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Products to order")]
    public List<ChatAICommandDTOCreateOrder_Product> Products { get; set; }
}

public record ChatAICommandDTOCreateOrder_Product
{
    [Required]
    [Description("Id of the stocked product")]
    public int StockedProductId { get; set; }
    [Required]
    [Description("How many units")]
    public long Quantity { get; set; }
}
