using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("update_recipe", "Update an existing recipe")]
public record ChatAICommandDTOUpdateRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe")]
    public int RecipeId { get; set; }
    [Required]
    [Description("Name of the recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("How many servings")]
    public int Serves { get; set; }
    [Required]
    [Description("List of ingredients")]
    public List<ChatAICommandDTOAddRecipeIngredients_Ingredient>? Ingredients { get; set; }
}

public record ChatAICommandDTOAddRecipeIngredients_Ingredient
{
    [Required]
    [Description("Name of the ingredient")]
    public string IngredientName { get; set; }
    [Required]
    [Description("How many units does the recipe call for")]
    public float Units { get; set; }
    [Required]
    [Description("Units type for the kitchen item")]
    public UnitType KitchenUnitType { get; set; }
}
