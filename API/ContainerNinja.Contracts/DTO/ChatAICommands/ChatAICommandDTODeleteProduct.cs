using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_stocked_products", "Deletes a list of stocked products")]
public record ChatAICommandDTODeleteStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of the stocked product to delete")]
    public List<ChatAICommandDTODeleteStockedProduct_Product> StockedProductsToDelete { get; set; }
}

public record ChatAICommandDTODeleteStockedProduct_Product
{
    [Required]
    [Description("Id of the stocked product")]
    public int StockedProductId { get; set; }
}