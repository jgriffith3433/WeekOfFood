namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandDeleteCookedRecipe : ChatAICommand
{
    public string Name { get; set; }
    public string Recipe
    {
        get { return Name; }
        set { Name = value; }
    }
}
