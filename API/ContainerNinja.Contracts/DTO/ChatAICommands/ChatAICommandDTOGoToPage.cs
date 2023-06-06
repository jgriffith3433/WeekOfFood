namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOGoToPage : ChatAICommandDTO
{
    public string Page { get; set; }
}
