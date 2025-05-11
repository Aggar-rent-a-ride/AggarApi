using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA.Constants.Enums;
using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DATA.DataAccess.Repositories
{
    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        public ReportRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Report>> FilterReportsAsync(int pageNo, int pageSize, TargetType? targetType, ReportStatus? status, DateRangePreset? date, OrderBy? sortingDirection, string[] includes = null)
        {
            var query = _context.Reports.AsQueryable();

            if(targetType != null)
                query = query.Where(r => r.TargetType == targetType);

            if (status != null)
                query = query.Where(r => r.Status == status);

            if (date != null)
            {
                var dateTime = DateTime.UtcNow;
                switch (date)
                {
                    case DateRangePreset.Today:
                        query = query.Where(r => r.CreatedAt.Date == dateTime.Date);
                        break;
                    case DateRangePreset.Yesterday:
                        query = query.Where(r => r.CreatedAt.Date == dateTime.AddDays(-1).Date);
                        break;
                    case DateRangePreset.Last7Days:
                        query = query.Where(r => r.CreatedAt >= dateTime.AddDays(-7));
                        break;
                    case DateRangePreset.Last30Days:
                        query = query.Where(r => r.CreatedAt >= dateTime.AddDays(-30));
                        break;
                    case DateRangePreset.Last365Days:
                        query = query.Where(r => r.CreatedAt >= dateTime.AddDays(-365));
                        break;
                    default:
                        break;
                }
            }

            if(sortingDirection == OrderBy.Descending)
                query = query.OrderByDescending(r => r.CreatedAt);
            else
                query = query.OrderBy(r => r.CreatedAt);

            query = query.Skip((pageNo - 1) * pageSize).Take(pageSize);

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.ToListAsync();
        }
    }
}
