using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("substitute_recipe_ingredient", "Substitutes an ingredient in a recipe")]
public record ChatAICommandDTOSubstituteRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("Name of the original ingredient")]
    public string OriginalIngredient { get; set; }
    [Required]
    [Description("Name of the new ingredient")]
    public string NewIngredient { get; set; }
}
