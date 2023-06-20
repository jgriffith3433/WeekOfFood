using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_stock" }, "Search for stocked product by name")]
public record ChatAICommandDTOSearchStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the stocked product to find")]
    public string StockedProductName { get; set; }
}
