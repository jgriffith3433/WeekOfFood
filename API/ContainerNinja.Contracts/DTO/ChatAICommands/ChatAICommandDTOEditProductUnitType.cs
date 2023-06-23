using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_product_unit_type", "Changes the product's unit type")]
public record ChatAICommandDTOEditProductUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the product to change the unit type of")]
    public int ProductId { get; set; }
    [Required]
    [Description("New unit type")]
    public UnitType KitchenUnitType { get; set; }
}
