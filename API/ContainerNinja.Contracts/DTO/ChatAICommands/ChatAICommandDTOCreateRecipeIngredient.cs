namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateRecipeIngredient : ChatAICommandArgumentsDTO
{
    public string Name { get; set; }
    public float? Units { get; set; }
    public string? UnitType { get; set; }
}
