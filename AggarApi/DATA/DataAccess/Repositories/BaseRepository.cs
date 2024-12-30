using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DATA.DataAccess.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AppDbContext _context;
        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Count() => await _context.Set<T>().CountAsync();

        public async Task<int> Count(Expression<Func<T, bool>> criteria) => 
            await _context.Set<T>().Where(criteria).CountAsync();

        public async Task<T> Find(Expression<Func<T, bool>> criteria) => 
            await _context.Set<T>().SingleOrDefaultAsync(criteria);

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> criteria, int pageNo, int pageSize) =>
            await _context.Set<T>().Where(criteria)
            .Skip((pageNo - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        public async Task<T> Get(int id) => await _context.Set<T>().FindAsync(id);

        public async Task<IEnumerable<T>> GetAll(int pageNo, int pageSize) =>
            _context.Set<T>()
            .Skip((pageNo - 1) * pageSize).Take(pageSize);

    }
}
