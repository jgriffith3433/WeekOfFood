namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteRecipe : ChatAICommandArgumentsDTO
{
    public string Name { get; set; }
}
