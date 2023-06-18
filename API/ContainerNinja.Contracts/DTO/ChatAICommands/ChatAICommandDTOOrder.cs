using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_order", "Creates a new order")]
public record ChatAICommandDTOOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new order")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Products to order")]
    public List<ChatAICommandDTOOrder_Product> Products { get; set; }
}

public record ChatAICommandDTOOrder_Product
{
    [Required]
    [Description("Id of the product")]
    public int ProductId { get; set; }
    [Required]
    [Description("How many units")]
    public long Quantity { get; set; }
}
