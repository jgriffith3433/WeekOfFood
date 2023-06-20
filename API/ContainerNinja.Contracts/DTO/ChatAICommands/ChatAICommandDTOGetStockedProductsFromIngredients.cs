using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_stocked_products_from_ingredients", "Gets stocked products from a list of ingredients")]
public record ChatAICommandDTOGetStockedProductsFromIngredients : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("IDs of the ingredients")]
    public int[] IngredientIds { get; set; }
}
