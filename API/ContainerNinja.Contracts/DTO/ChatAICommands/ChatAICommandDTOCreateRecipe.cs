namespace ContainerNinja.Contracts.DTO.ChatAICommands;

public record ChatAICommandDTOCreateRecipe : ChatAICommandArgumentsDTO
{
    public string Name { get; set; }
    public int? Serves { get; set; }
    public List<ChatAICommandDTOCreateRecipeIngredient> Ingredients { get; set; }
}
