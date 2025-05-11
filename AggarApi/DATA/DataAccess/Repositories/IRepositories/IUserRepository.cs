using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA.Constants.Enums;
using DATA.Models;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IUserRepository: IBaseRepository<AppUser>
    {
        Task<(IEnumerable<AppUser> appUsers, int Count)> GetTotalUsersAsync(string? role, int pageNo, int pageSize, DateRangePreset? dateFilter);
    }
}
