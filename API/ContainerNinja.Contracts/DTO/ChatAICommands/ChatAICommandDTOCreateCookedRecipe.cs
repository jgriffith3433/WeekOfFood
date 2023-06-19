using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_logged_recipe", "Create a new log for a recipe that was used")]
public record ChatAICommandDTOCreateCookedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new logged recipe")]
    public bool? UserGavePermission { get; set; }

    [Required]
    [Description("Id recipe to log")]
    public int RecipeId { get; set; }

    [Required]
    [Description("When did the user make the recipe in DateTime format")]
    public DateTime? When { get; set; }
}
