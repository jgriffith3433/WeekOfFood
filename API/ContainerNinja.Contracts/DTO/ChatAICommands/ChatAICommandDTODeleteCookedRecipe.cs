namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTODeleteCookedRecipe : ChatAICommandArgumentsDTO
{
    public string Name { get; set; }
}
