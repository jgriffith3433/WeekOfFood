namespace ContainerNinja.Contracts.ChatAI;

public record ChatAICommandCookedRecipeSubstituteIngredient : ChatAICommand
{
    public string Recipe { get; set; }

    public string Original { get; set; }
    public string Ingredient
    {
        get { return Original; }
        set { Original = value; }
    }

    public string New { get; set; }
    public string Substitute
    {
        get { return New; }
        set { New = value; }
    }
}
