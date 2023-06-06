namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteCookedRecipeIngredient : ChatAICommandDTO
{
    public string Recipe { get; set; }
    public string Ingredient { get; set; }
}
