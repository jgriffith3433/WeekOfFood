namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOAddCookedRecipeIngredient : ChatAICommandArgumentsDTO
{
    public string RecipeName { get; set; }
    public string IngredientName { get; set; }
    public int Units { get; set; }
    public string UnitType { get; set; }
}
