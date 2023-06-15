using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_recipe_name", "Edit a recipe's name")]
public record ChatAICommandDTOEditRecipeName : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe")]
    public string OriginalName { get; set; }
    [Required]
    [Description("New name")]
    public string NewName { get; set; }
}
