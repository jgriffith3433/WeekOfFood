using ContainerNinja.Contracts.Common;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_recipes" }, "Returns a list of recipes matching search")]
public record ChatAICommandDTOSearchRecipes : ChatAICommandArgumentsDTO
{
    [Description("Name to search for")]
    public string Search { get; set; }
}
