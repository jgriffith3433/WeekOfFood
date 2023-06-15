namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateProduct : ChatAICommandArgumentsDTO
{
    public string Product { get; set; }
}
