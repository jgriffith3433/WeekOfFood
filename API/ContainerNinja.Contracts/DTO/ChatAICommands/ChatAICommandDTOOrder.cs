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
    [Description("Items to order")]
    public List<ChatAICommandDTOOrderItem> Items { get; set; }
}

public record ChatAICommandDTOOrderItem : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the item")]
    public string ItemName { get; set; }
    [Required]
    [Description("How many units")]
    public long Quantity { get; set; }
}
