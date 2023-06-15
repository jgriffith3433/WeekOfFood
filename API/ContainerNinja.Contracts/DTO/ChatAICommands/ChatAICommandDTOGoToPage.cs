namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOGoToPage : ChatAICommandArgumentsDTO
{
    public string Page { get; set; }
}
