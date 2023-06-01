namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandGoToPage : ChatAICommand
{
    public string Page { get; set; }
}
