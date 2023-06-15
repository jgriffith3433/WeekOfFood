using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_recipe_ingredients", "Gets all of the ingredients for a recipe")]
public record ChatAICommandDTOGetRecipeIngredients : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the recipe to get ingredients for")]
    public string RecipeName { get; set; }
}
