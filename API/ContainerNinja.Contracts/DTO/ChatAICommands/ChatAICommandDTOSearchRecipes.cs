namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOSearchRecipes : ChatAICommandArgumentsDTO
{
    public string Search { get; set; }
}
