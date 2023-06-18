using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "get_all_recipes" }, "Returns a list of all recipes")]
public record ChatAICommandDTOGetAllRecipes : ChatAICommandArgumentsDTO
{
    [Description("Name to search for")]
    public string Search { get; set; }
}
