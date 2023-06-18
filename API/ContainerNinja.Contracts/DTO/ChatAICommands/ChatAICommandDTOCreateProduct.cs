using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_product", "Create a new product")]
public record ChatAICommandDTOCreateProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new product")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the product")]
    public string ProductName { get; set; }
}
