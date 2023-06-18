using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//commented out to prevent reflection from adding it to the json schema
//[ChatCommandSpecification("template", "teamplteescriptiontext")]
public record ChatAICommandDTOTemplate : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to template")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the templateproperty")]
    public string TemplateProperty { get; set; }
}
