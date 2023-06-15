using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_todo_list", "Create a new to do list")]
public record ChatAICommandDTOAddTodoList : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the list")]
    public string ListName { get; set; }
}
