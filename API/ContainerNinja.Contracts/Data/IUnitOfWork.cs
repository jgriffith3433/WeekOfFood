using ContainerNinja.Contracts.Data.Repositories;

namespace ContainerNinja.Contracts.Data
{
    public interface IUnitOfWork
    {
        IItemRepository Items { get; }
        ITodoListRepository TodoLists { get; }
        IUserRepository Users { get; }
        IChatCommandRepository ChatCommands { get; }
        IChatConversationRepository ChatConversations { get; }
        IProductRepository Products { get; }
        ICompletedOrderRepository CompletedOrders { get; }
        ICompletedOrderProductRepository CompletedOrderProducts { get; }
        IProductStockRepository ProductStocks { get; }
        ICalledIngredientRepository CalledIngredients { get; }
        IRecipeRepository Recipes { get; }
        ICookedRecipeRepository CookedRecipes { get; }
        ICookedRecipeCalledIngredientRepository CookedRecipeCalledIngredients { get; }
        Task CommitAsync();
    }
}