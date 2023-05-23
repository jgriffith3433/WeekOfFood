﻿using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Core.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }
        public IItemRepository Items => new ItemRepository(_context);

        public ITodoListRepository TodoLists => new TodoListRepository(_context);

        public IUserRepository Users => new UserRepository(_context);

        public IChatCommandRepository ChatCommands => new ChatCommandRepository(_context);

        public IChatConversationRepository ChatConversations => new ChatConversationRepository(_context);

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}