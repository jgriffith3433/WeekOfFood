using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_recipe_name", "Change a recipe's name")]
public record ChatAICommandDTOEditRecipeName : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to change the recipe's name")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Id of the recipe")]
    public int Id { get; set; }
    [Required]
    [Description("New name")]
    public string NewName { get; set; }
}
