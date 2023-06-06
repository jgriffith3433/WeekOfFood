namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateRecipeIngredient : ChatAICommandDTO
{
    public string Name { get; set; }
    public float? Units { get; set; }
    public string? UnitType { get; set; }
}
