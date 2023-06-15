namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteRecipeIngredient : ChatAICommandArgumentsDTO
{
    public string Recipe { get; set; }
    public string Ingredient { get; set; }
}
