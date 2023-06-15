namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteCookedRecipeIngredient : ChatAICommandArgumentsDTO
{
    public string RecipeName { get; set; }
    public string IngredientName { get; set; }
}
