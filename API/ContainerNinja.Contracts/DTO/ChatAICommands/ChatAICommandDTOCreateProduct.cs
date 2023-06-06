namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateProduct : ChatAICommandDTO
{
    public string Product { get; set; }
}
