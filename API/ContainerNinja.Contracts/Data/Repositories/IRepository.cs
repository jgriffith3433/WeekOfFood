using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ContainerNinja.Contracts.Data.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        IIncludableQueryable<T, TProperty> Include<TEntity, TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath) where TEntity : class;
        int Count();
    }
}