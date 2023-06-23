using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_logged_recipe_ingredient", "Deletes a logged recipe ingredient")]
public record ChatAICommandDTODeleteCookedRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the logged recipe")]
    public int LoggedRecipeId { get; set; }
    [Required]
    [Description("Name of the ingredient to delete")]
    public string IngredientName { get; set; }
}
