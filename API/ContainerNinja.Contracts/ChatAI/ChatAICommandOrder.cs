namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandOrder : ChatAICommand
{
    public List<ChatAICommandOrderItem> Items { get; set; }
}
