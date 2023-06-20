using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification(new string[] { "search_for_recipes_with_stocked_products" }, "Returns a list of recipes that a user can use with a list off stocked products")]
public record ChatAICommandDTOSearchForRecipesWithStockedProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Stocked products to search recipes with")]
    public List<ChatAICommandDTOSearchForRecipesWithIngredients_ProductStock> StockedProducts { get; set; }
}

public record ChatAICommandDTOSearchForRecipesWithIngredients_ProductStock
{
    [Required]
    [Description("Id of stocked product")]
    public int StockedProductId { get; set; }
}
