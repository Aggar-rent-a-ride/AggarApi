using System.Linq.Expressions;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> Get(int id);
        Task<IEnumerable<T>> GetAll(int pageNo, int pageSize);
        Task<int> Count();
        Task<int> Count(Expression<Func<T, bool>> criteria);
        Task<T> Find(Expression<Func<T, bool>> criteria, string[] includes=null);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> criteria, int pageNo, int pageSize, string[] includes = null);
        Task<T> AddOrUpdate(T entity);
        void Delete(T entity);
        void Attach(T entity);
        Task<IEnumerable<T>> AddRange(IEnumerable<T> entities);
    }
}
