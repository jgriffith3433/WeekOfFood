namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOEditRecipeName : ChatAICommandArgumentsDTO
{
    public string Original { get; set; }
    public string New { get; set; }
}
