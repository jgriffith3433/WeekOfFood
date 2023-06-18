using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_recipe", "Create a new recipe")]
public record ChatAICommandDTOCreateRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new recipe")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("How many servings")]
    public int? Serves { get; set; }
    [Required]
    [Description("List of ingredients")]
    public List<ChatAICommandDTOCreateRecipeIngredient>? Ingredients { get; set; }
}

public record ChatAICommandDTOCreateRecipeIngredient
{
    [Required]
    [Description("Name of the ingredient")]
    public string IngredientName { get; set; }
    [Required]
    [Description("How many units does the recipe call for")]
    public float? Units { get; set; }
    [Required]
    [Description("Type of unit")]
    public string? UnitType { get; set; }
}
