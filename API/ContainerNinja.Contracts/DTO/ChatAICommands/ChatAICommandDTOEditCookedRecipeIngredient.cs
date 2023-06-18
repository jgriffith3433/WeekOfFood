using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_logged_recipe_ingredient", "Delete one ingredient and add another")]
public record ChatAICommandDTOEditCookedRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to substitute an ingredient in the logged recipe")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the logged recipe")]
    public int LoggedRecipeId { get; set; }
    [Required]
    [Description("Id of the logged ingredient to delete")]
    public int LoggedIngredientId { get; set; }
    [Required]
    [Description("Name of the new ingredient")]
    public string NewIngredientName { get; set; }
}
