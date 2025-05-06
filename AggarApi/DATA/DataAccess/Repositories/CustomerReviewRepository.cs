using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore;

namespace DATA.DataAccess.Repositories
{
    public class CustomerReviewRepository : BaseRepository<CustomerReview>, ICustomerReviewRepository
    {
        public CustomerReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<(double Behavior, double Punctuality, double Truthfulness)>> GetAllRatesAsync(Expression<Func<CustomerReview, bool>> criteria)
        {
            var result = await _context.CustomerReviews.Where(criteria)
                .Select(r => new
                {
                    Behavior = r.Behavior,
                    Punctuality = r.Punctuality,
                    Truthfulness = r.Truthfulness
                })
                .ToListAsync();
            return result == null ? new List<(double Behavior, double Punctuality, double Truthfulness)>() : result.Select(r => (r.Behavior, r.Punctuality, r.Truthfulness)).ToList();
        }
    }
}
