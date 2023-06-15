namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOOrderItem : ChatAICommandArgumentsDTO
{
    public string Name { get; set; }
    public long Quantity { get; set; }
}
