using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_logged_recipe_date", "Changes the date for the logged recipe")]
public record ChatAICommandDTOEditCookedRecipeDate : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the logged recipe")]
    public int LoggedRecipeId { get; set; }
    [Required]
    [Description("When the recipe was used")]
    public DateTime? When { get; set; }
}
