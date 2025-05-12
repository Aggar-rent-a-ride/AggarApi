using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs.IBackgroundJobs
{
    public interface IUserManagementJob
    {
        Task ScheduleUserUnbanAsync(int userId, DateTime bannedTo);
    }
}
