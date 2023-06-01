namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandAddCookedRecipeIngredient : ChatAICommand
{
    public string Recipe { get; set; }
    public string Name { get; set; }
    public string Ingredient
    {
        get { return Name; }
        set { Name = value; }
    }

    public int Units { get; set; }
    public string UnitType { get; set; }
}
