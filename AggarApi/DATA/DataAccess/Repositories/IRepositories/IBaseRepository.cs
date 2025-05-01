using DATA.Constants;
using DATA.Constants.Enums;
using System.Linq.Expressions;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetAsync(int id);
        Task<T?> GetFirstAsync();
        Task<T?> GetAsync(int id, string[] includes=null);
        Task<IEnumerable<T>> GetAllAsync(int pageNo, int pageSize, Expression<Func<T, object>> sortingExpression = null, OrderBy sortingDirection = OrderBy.Ascending);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> criteria);
        Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes=null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> criteria, int pageNo, int pageSize, string[] includes = null, Expression<Func<T, object>> sortingExpression = null, OrderBy sortingDirection = OrderBy.Ascending);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null, Expression<Func<T, object>> sortingExpression = null, OrderBy sortingDirection = OrderBy.Ascending);
        Task<T> AddOrUpdateAsync(T entity);
        void Delete(T entity);
        void Attach(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task<bool> CheckAllAsync(Expression<Func<T, bool>> criteria, string[] includes);
        Task<bool> CheckAnyAsync(Expression<Func<T, bool>> criteria, string[] includes);
    }
}
