using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.Data.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        IEnumerable<Product> SearchForByName(string search);
    }
}