namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateRecipe : ChatAICommandDTO
{
    public string Name { get; set; }
    public int? Serves { get; set; }
    public List<ChatAICommandDTOCreateRecipeIngredient> Ingredients { get; set; }
}
