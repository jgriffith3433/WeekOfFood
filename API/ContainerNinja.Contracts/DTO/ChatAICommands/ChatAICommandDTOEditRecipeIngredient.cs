using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_recipe_ingredient", "Remove an ingredient and add another in a recipe")]
public record ChatAICommandDTOEditRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to substitute an ingredient in the recipe")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Id of the recipe")]
    public int Id { get; set; }
    [Required]
    [Description("Name of the original ingredient")]
    public string OriginalIngredient { get; set; }
    [Required]
    [Description("Name of the new ingredient")]
    public string NewIngredient { get; set; }
}
