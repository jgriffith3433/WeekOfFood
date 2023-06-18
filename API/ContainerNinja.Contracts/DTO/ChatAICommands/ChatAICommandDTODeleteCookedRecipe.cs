using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_logged_recipe", "Deletes a logged recipe")]
public record ChatAICommandDTODeleteCookedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to delete the recipe log")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Id of the recipe log")]
    public int Id { get; set; }
}
