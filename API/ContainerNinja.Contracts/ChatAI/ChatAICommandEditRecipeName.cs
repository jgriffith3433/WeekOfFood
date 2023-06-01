namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandEditRecipeName : ChatAICommand
{
    public string Original { get; set; }
    public string New { get; set; }
}
