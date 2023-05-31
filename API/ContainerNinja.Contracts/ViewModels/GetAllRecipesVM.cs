using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllRecipesVM
    {
        public List<RecipeDTO> Recipes { get; set; }
        public List<UnitTypeDTO> UnitTypes { get; set; }
    }
}