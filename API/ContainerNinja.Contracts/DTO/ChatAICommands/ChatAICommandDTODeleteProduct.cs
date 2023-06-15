using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_product", "Deletes a product")]
public record ChatAICommandDTODeleteProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the product")]
    public string ProductName { get; set; }
}
