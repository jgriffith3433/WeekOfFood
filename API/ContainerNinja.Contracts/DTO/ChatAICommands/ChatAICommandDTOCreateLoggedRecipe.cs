using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_new_logged_recipe", "Create a new log for a recipe that was used")]
public record ChatAICommandDTOCreateLoggedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe to create a log record for")]
    public int RecipeId { get; set; }
}
