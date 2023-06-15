using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_logged_recipe_ingredient_unit_type", "Changes the unit type for the logged recipe's ingredient")]
public record ChatAICommandDTOEditCookedRecipeIngredientUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("Name of the ingredient")]
    public string IngredientName { get; set; }
    [Required]
    [Description("New unit type")]
    public string UnitType { get; set; }
}
