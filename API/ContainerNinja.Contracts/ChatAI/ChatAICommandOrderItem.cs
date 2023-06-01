namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandOrderItem : ChatAICommand
{
    public string Name { get; set; }
    public long Quantity { get; set; }
}
