using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_kitchen_products", "Create kitchen product records when managing kitchen inventory")]
public record ChatAICommandDTOCreateStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of kitchen products the user is taking stock of")]
    public List<ChatAICommandDTOCreateStockedProducts_KitchenProduct> KitchenProducts { get; set; }
}

public record ChatAICommandDTOCreateStockedProducts_KitchenProduct
{
    [Required]
    [Description("Name of the kitchen product")]
    public string KitchenProductName { get; set; }
    [Required]
    [Description("How many units do they have in stock")]
    public float Units { get; set; }
    [Required]
    [Description("Units type for the kitchen item")]
    public UnitType KitchenUnitType { get; set; }
}