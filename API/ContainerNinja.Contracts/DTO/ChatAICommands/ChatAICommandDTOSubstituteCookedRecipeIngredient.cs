using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("substitute_logged_recipe_ingredient", "Substitutes an ingredient in a logged recipe")]
public record ChatAICommandDTOSubstituteCookedRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the logged recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("Name of the original ingredient")]
    public string OriginalIngredient { get; set; }
    [Required]
    [Description("Name of the new ingredient")]
    public string NewIngredient { get; set; }
}
