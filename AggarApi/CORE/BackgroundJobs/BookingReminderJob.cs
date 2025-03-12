using CORE.Services.IServices;
using DATA.Constants;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs
{
    public class BookingReminderJob
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingReminderJob> _logger;
        public BookingReminderJob(IEmailService emailService, IEmailTemplateRendererService emailTemplateRendererService, IUnitOfWork unitOfWork, ILogger<BookingReminderJob> logger)
        {
            _emailService = emailService;
            _emailTemplateRendererService = emailTemplateRendererService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SendBookingAcceptedNotification(DateTime bookingStartDate, int userId)
        {
            try
            {
                var user = await _unitOfWork.AppUsers.GetAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return;
                }
                // Prepare email content
                var emailContent = await _emailTemplateRendererService.RenderTemplateAsync(
                Templates.Notification,
                new
                {
                    NotificationContent = $"Your booking on {bookingStartDate:d} is coming up!",
                    NotificationType = NotificationType.BookingAccepted
                });

                // Send email
                await _emailService.SendEmailAsync(
                    user.Email,
                    EmailSubject.NotificationReceived,
                    emailContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending booking reminder email.");
            }
        }
    }
}
