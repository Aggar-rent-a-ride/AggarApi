using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.Constants;
using CORE.DTOs;
using DATA.DataAccess.Repositories.UnitOfWork;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CORE.BackgroundJobs
{
    public class UserManagementJob : IUserManagementJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserManagementJob> _logger;
        private readonly IRecurringJobManager _recurringJobManager;
        public UserManagementJob(IUnitOfWork unitOfWork, ILogger<UserManagementJob> logger, IRecurringJobManager recurringJobManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _recurringJobManager = recurringJobManager;
        }
        public async Task ScheduleUserUnbanAsync(int userId, DateTime bannedTo)
        {
            string jobId = $"unban-user-{userId}";

            // Schedule to run at the specified time
            BackgroundJob.Schedule(() => UnbanUserAsync(userId, jobId), bannedTo);
        }
        private void RescheduleUserUnban(string jobId, int userId)
        {
            // Reschedule for tomorrow
            _recurringJobManager.AddOrUpdate(jobId,
                () => UnbanUserAsync(userId, jobId),
                Cron.Daily());
        }
        public async Task UnbanUserAsync(int userId, string jobId)
        {
            var user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found. No action taken.");
                return;
            }

            if(user.Status != DATA.Models.Enums.UserStatus.Banned)
            {
                _logger.LogInformation($"User {userId} is not banned. No action taken.");
                return;
            }

            user.Status = DATA.Models.Enums.UserStatus.Active;
            user.BannedTo = null;
            
            var changes = await _unitOfWork.CommitAsync();
            if(changes == 0)
            {
                _logger.LogWarning($"Failed to unban user {userId}. No changes made.");
                RescheduleUserUnban(jobId, userId);
                return;
            }

            _logger.LogInformation($"User {userId} unbanned successfully.");
            _recurringJobManager.RemoveIfExists(jobId);
        }
    }
}
