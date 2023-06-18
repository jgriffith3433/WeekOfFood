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
    [Description("Id of the recipe to change the name of")]
    public int RecipeId { get; set; }
    [Required]
    [Description("New name")]
    public string NewName { get; set; }
}
