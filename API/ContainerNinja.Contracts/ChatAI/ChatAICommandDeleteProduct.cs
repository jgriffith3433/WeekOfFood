namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandDeleteProduct : ChatAICommand
{
    public string Product { get; set; }
}
