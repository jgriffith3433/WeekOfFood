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
        Task CommitAsync();
    }
}