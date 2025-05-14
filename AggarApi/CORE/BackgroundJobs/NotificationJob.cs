using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.Constants;
using CORE.DTOs.Notification;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CORE.BackgroundJobs
{
    public class NotificationJob : INotificationJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationJob> _logger;
        private readonly INotificationService _notificationService;
        private readonly ISignalRNotificationService _signalRNotificationService;

        public NotificationJob(IUnitOfWork unitOfWork, ILogger<NotificationJob> logger, INotificationService notificationService, ISignalRNotificationService signalRNotificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
            _signalRNotificationService = signalRNotificationService;
        }

        public async Task ExecuteAsync(CreateNotificationDto dto)
        {
            BackgroundJob.Enqueue(() => CreateAndSendNotification(dto));
        }
        public async Task CreateAndSendNotification(CreateNotificationDto dto)
        {
            var notificationResponse = await _notificationService.CreateNotificationAsync(dto);
            if(notificationResponse.StatusCode != StatusCodes.Created)
            {
                _logger.LogError("Failed to create notification");
                return;
            }

            var notification = notificationResponse.Data;
            await _signalRNotificationService.SendNotificationAsync(notification, dto.ReceiverId);
        }
    }
}
