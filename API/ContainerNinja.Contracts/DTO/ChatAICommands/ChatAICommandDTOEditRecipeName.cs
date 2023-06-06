namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOEditRecipeName : ChatAICommandDTO
{
    public string Original { get; set; }
    public string New { get; set; }
}
