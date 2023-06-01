namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandDeleteRecipe : ChatAICommand
{
    public string Name { get; set; }
}
