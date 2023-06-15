using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_logged_recipe", "Create a new log for a recipe that was used")]
public record ChatAICommandDTOCreateCookedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe to log")]
    public string RecipeName { get; set; }
}
