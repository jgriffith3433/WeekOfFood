namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteRecipeIngredient : ChatAICommandDTO
{
    public string Recipe { get; set; }
    public string Ingredient { get; set; }
}
