namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOOrder : ChatAICommandDTO
{
    public List<ChatAICommandDTOOrderItem> Items { get; set; }
}
