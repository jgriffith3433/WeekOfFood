namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteProduct : ChatAICommandArgumentsDTO
{
    public string Product { get; set; }
}
