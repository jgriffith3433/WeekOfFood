using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_recipe_id", "Gets the id for a recipe by name")]
public record ChatAICommandDTOGetRecipeId : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe to get an id for")]
    public string RecipeName { get; set; }
}
