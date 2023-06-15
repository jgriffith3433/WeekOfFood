using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("search_recipes", "Searches for recipes by name")]
public record ChatAICommandDTOSearchRecipes : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name to search for")]
    public string Search { get; set; }
}
