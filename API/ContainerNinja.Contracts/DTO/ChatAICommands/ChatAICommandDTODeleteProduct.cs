namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteProduct : ChatAICommandDTO
{
    public string Product { get; set; }
}
