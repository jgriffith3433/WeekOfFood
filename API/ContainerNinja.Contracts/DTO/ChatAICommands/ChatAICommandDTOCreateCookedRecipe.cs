namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateCookedRecipe : ChatAICommandDTO
{
    public string Name { get; set; }
    public string Recipe
    {
        get { return Name; }
        set { Name = value; }
    }
}
