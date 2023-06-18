using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "get_stocked_products" }, "Get a list of stocked products")]
public record ChatAICommandDTOGetStockedProducts : ChatAICommandArgumentsDTO
{
    [Description("Name to search for")]
    public string Search { get; set; }
}
