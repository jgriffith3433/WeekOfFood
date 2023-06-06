namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOSubstituteCookedRecipeIngredient : ChatAICommandDTO
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
