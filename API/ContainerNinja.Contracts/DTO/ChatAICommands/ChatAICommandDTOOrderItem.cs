namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOOrderItem : ChatAICommandDTO
{
    public string Name { get; set; }
    public long Quantity { get; set; }
}
