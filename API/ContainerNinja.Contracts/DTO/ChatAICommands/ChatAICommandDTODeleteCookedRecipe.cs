using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_logged_recipe", "Deletes a logged recipe")]
public record ChatAICommandDTODeleteCookedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of logged recipe")]
    public string RecipeName { get; set; }
}
