namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandDeleteRecipeIngredient : ChatAICommand
{
    public string Recipe { get; set; }
    public string Ingredient { get; set; }
}
