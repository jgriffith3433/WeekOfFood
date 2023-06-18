using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_todo_list", "Create a new to do list")]
public record ChatAICommandDTOAddTodoList : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to create a new to do list")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the list")]
    public string ListName { get; set; }
}
