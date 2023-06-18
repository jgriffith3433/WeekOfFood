using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_stocked_product", "Deletes a stocked product")]
public record ChatAICommandDTODeleteStockedProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to delete the product")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Id of the stocked product")]
    public int StockedProductId { get; set; }
}
