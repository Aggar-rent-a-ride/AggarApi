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
    public class RenterReviewRepository : BaseRepository<RenterReview>, IRenterReviewRepository
    {
        public RenterReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<(double Behavior, double Punctuality, double Care)>> GetAllRatesAsync(Expression<Func<RenterReview, bool>> criteria)
        {
            var result = await _context.RenterReviews.Where(criteria)
                .Select(r => new
                {
                    Behavior = r.Behavior,
                    Punctuality = r.Punctuality,
                    Care = r.Care
                })
                .ToListAsync();
            return result == null ? new List<(double Behavior, double Punctuality, double Care)>() : result.Select(r => (r.Behavior, r.Punctuality, r.Care)).ToList();
        }
    }
}
