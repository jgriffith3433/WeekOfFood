using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("search_stocked_products", "Gets the ID for a stocked product by name")]
public record ChatAICommandDTOGetStockedProductId : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the stocked product to get an ID for")]
    public string StockedProductName { get; set; }
}
