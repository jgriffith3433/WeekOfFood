using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("link_recipe_ingredient_to_stocked_product", "Links a recipe's ingredient to a stocked product by ID")]
public record ChatAICommandDTOLinkRecipeIngredientToStockedProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new to do list")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("ID of the recipe")]
    public int RecipeId { get; set; }
    [Required]
    [Description("ID of the ingredient")]
    public int IngredientId { get; set; }
    [Required]
    [Description("ID of the stocked product")]
    public long? StockedProductId { get; set; }
}
