namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandCreateRecipeIngredient : ChatAICommand
{
    public string Name { get; set; }
    public float? Units { get; set; }
    public string? UnitType { get; set; }
}
