using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_stocked_products" }, "Search for stocked products by name")]
public record ChatAICommandDTOSearchStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name to search for")]
    public string Search { get; set; }
}
