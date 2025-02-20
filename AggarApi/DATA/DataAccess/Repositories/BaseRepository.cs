using Azure;
using DATA.Constants;
using DATA.Constants.Enums;
using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DATA.DataAccess.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AppDbContext _context;
        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> AddOrUpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            return entities;
        }

        public void Attach(T entity)
        {
            _context.Set<T>().Attach(entity);
        }

        public async Task<int> CountAsync() => await _context.Set<T>().CountAsync();

        public async Task<int> CountAsync(Expression<Func<T, bool>> criteria) => 
            await _context.Set<T>().Where(criteria).CountAsync();

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.SingleOrDefaultAsync(criteria);
        }


        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> criteria, int pageNo, int pageSize, string[] includes, Expression<Func<T, object>> sortingExpression = null, OrderBy sortingDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().Where(criteria);
            
            if(sortingExpression != null)
            {
                if (sortingDirection == OrderBy.Ascending)
                    query = query.OrderBy(sortingExpression);
                else
                    query = query.OrderByDescending(sortingExpression);
            }

            if(includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.Skip((pageNo - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        }


        public async Task<T?> GetAsync(int id) => await _context.Set<T>().FindAsync(id);

        public async Task<T?> GetAsync(int id, string[] includes = null)
        {
            T? entity = await _context.Set<T>().FindAsync(id);

            if (includes != null)
                foreach (var include in includes)
                    await _context.Entry(entity).Collection(include).LoadAsync();

            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync(int pageNo, int pageSize, Expression<Func<T, object>> sortingExpression = null, OrderBy sortingDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>();

            if (sortingExpression != null)
            {
                if (sortingDirection == OrderBy.Ascending)
                    query = query.OrderBy(sortingExpression);
                else
                    query = query.OrderByDescending(sortingExpression);
            }

            return query.Skip((pageNo - 1) * pageSize).Take(pageSize);
        }


        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null, Expression<Func<T, object>> sortingExpression = null, OrderBy sortingDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().Where(criteria);

            if (sortingExpression != null)
            {
                if (sortingDirection == OrderBy.Ascending)
                    query = query.OrderBy(sortingExpression);
                else
                    query = query.OrderByDescending(sortingExpression);
            }

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.ToListAsync();
        }

        public async Task<bool> CheckAllAsync(Expression<Func<T, bool>> criteria, string[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.AllAsync(criteria);
        }

        public async Task<bool> CheckAnyAsync(Expression<Func<T, bool>> criteria, string[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.AnyAsync(criteria);
        }
    }
}
