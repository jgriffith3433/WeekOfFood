using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_logged_recipes", "get_logged_recipe" }, "Searches for recipe logs by name")]
public record ChatAICommandDTOSearchCookedRecipes : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name to search for")]
    public string Search { get; set; }
}
