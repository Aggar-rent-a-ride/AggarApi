using System.Linq.Expressions;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync(int pageNo, int pageSize);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> criteria);
        Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes=null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> criteria, int pageNo, int pageSize, string[] includes = null);
        Task<T> AddOrUpdateAsync(T entity);
        void Delete(T entity);
        void Attach(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    }
}
