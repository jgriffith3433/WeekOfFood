using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_product_unit_type", "Changes the product's unit type")]
public record ChatAICommandDTOEditProductUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the product")]
    public string ProductName { get; set; }
    [Required]
    [Description("New unit type")]
    public string UnitType { get; set; }
}
