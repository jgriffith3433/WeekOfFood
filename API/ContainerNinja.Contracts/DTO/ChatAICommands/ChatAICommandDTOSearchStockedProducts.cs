using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_kitchen_products" }, "Search for kitchen products by name")]
public record ChatAICommandDTOSearchStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of names to search for")]
    public List<ChatAICommandDTOSearchStockedProducts_Search> ListOfNames { get; set; }
}

public record ChatAICommandDTOSearchStockedProducts_Search
{
    [Required]
    [Description("Name of the kitchen product to find")]
    public string StockedProductName { get; set; }
}
