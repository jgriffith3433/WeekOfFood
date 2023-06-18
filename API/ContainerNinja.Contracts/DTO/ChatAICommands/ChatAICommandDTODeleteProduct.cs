using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_product", "Deletes a product")]
public record ChatAICommandDTODeleteProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to delete the product")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the product")]
    public string ProductName { get; set; }
}
