using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_recipe_ingredient", "Deletes a recipe ingredient")]
public record ChatAICommandDTODeleteRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("Name of the ingredient to delete")]
    public string IngredientName { get; set; }
}
