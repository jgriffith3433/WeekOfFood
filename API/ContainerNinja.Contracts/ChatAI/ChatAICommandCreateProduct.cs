namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandCreateProduct : ChatAICommand
{
    public string Product { get; set; }
}
