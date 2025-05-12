using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA.Constants.Enums;
using DATA.Models;
using DATA.Models.Enums;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IReportRepository : IBaseRepository<Report>
    {
        Task<IEnumerable<Report>> FilterReportsAsync(int pageNo, int pageSize, TargetType? targetType, ReportStatus? status, DateRangePreset? date, OrderBy? sortingDirection, string[] includes = null);
        Task<int> FilterReportsCountAsync(TargetType? targetType, ReportStatus? status, DateRangePreset? date);
    }
}
