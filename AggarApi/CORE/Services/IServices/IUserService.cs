using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IUserService
    {
        Task<bool> CheckAnyAsync(int userId);
        Task<bool> CheckAllUsersExist(List<int> userIds);
    }
}
