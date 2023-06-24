using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("update_logged_recipe", "Update logged recipe")]
public record ChatAICommandDTOUpdateLoggedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the logged recipe")]
    public int LoggedRecipeId { get; set; }

    [Required]
    [Description("When did the user make the recipe in DateTime format")]
    public DateTime? When { get; set; }

    [Required]
    [Description("List of ingredients in the logged recipe")]
    public List<ChatAICommandDTOUpdateLoggedRecipe_Ingredient> Ingredients { get; set; }
}

public record ChatAICommandDTOUpdateLoggedRecipe_Ingredient
{
    [Required]
    [Description("Name of the ingredient")]
    public string IngredientName { get; set; }
    [Required]
    [Description("How many units of the ingredient as a number")]
    public float Quantity { get; set; }
    [Required]
    [Description("Kitchen unit type for the kitchen product")]
    public KitchenUnitType KitchenUnitType { get; set; }
}