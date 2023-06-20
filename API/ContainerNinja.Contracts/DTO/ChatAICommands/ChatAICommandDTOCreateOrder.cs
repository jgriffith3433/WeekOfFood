using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_order", "Creates a new order")]
public record ChatAICommandDTOCreateOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new order")]
    public bool? UserGavePermission { get; set; }
}