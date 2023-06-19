using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_walmart_products" }, "Searches for a walmart product by name")]
public record ChatAICommandDTOSearchWalmartProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name to search for")]
    public string Search { get; set; }
}
