using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(DatabaseContext context) : base(context)
        {
        }

        //public IEnumerable<Product> Search(Func<Product, bool> query)
        public IEnumerable<Product> SearchForByName(string search)
        {
            return from p in _dbSet.AsQueryable() where EF.Functions.Like(p.Name, string.Format("%{0}%", search)) select p;
        }
    }
}