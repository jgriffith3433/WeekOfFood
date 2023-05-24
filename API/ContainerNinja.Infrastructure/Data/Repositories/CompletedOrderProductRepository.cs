using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class CompletedOrderProductRepository : Repository<CompletedOrderProduct>, ICompletedOrderProductRepository
    {
        public CompletedOrderProductRepository(DatabaseContext context) : base(context)
        {
        }
    }
}