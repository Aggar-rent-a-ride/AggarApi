using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace CORE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResponseDto<object>> Acknowledge(HashSet<int> notificationIds, int userId)
        {
            var notifications = await _unitOfWork.Notifications.GetAllAsync(n => n.ReceiverId == userId && n.IsSeen == false && notificationIds.Contains(n.Id));

            if(notifications.Any() == false)
                _logger.LogWarning("No notifications found to acknowledge");

            foreach (var notification in notifications)
                notification.IsSeen = true;

            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                _logger.LogWarning("Failed to acknowledge notifications");


            _logger.LogInformation("Notifications acknowledged successfully");
            return new ResponseDto<object>
            {
                StatusCode = 200,
                Message = "Notifications acknowledged successfully",
            };
        }
    }
}
