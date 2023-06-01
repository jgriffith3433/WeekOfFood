namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandDeleteCookedRecipeIngredient : ChatAICommand
{
    public string Recipe { get; set; }
    public string Ingredient { get; set; }
}
