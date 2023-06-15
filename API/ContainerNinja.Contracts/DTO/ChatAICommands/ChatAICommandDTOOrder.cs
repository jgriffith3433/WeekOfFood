namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOOrder : ChatAICommandArgumentsDTO
{
    public List<ChatAICommandDTOOrderItem> Items { get; set; }
}
