using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ContainerNinja.Contracts.Common;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_logged_recipe_ingredient", "Add a new ingredient to a logged recipe")]
public record ChatAICommandDTOAddCookedRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to add an ingredient to the logged recipe")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the logged recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("Name of the ingredient")]
    public string IngredientName { get; set; }
    [Required]
    [Description("How many units of the ingredient")]
    public int Units { get; set; }
    [Required]
    [Description("Type of unit")]
    public string UnitType { get; set; }
}
