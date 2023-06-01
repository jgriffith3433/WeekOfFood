namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandCreateRecipe : ChatAICommand
{
    public string Name { get; set; }
    public int? Serves { get; set; }
    public List<ChatAICommandCreateRecipeIngredient> Ingredients { get; set; }
}
