namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateCookedRecipe : ChatAICommandArgumentsDTO
{
    public string RecipeName { get; set; }
}
