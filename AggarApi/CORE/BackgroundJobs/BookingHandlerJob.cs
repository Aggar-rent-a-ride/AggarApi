using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.Constants.Includes;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs
{
    public class BookingHandlerJob : IBookingHandlerJob
    {
        private readonly ILogger<BookingHandlerJob> _logger;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationJob _notificationJob;
        private readonly IEmailSendingJob _emailSendingJob;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;

        public BookingHandlerJob(IUnitOfWork unitOfWork, IRecurringJobManager recurringJobManager, ILogger<BookingHandlerJob> logger, INotificationJob notificationJob, IEmailSendingJob emailSendingJob, IEmailTemplateRendererService emailTemplateRendererService)
        {
            _unitOfWork = unitOfWork;
            _recurringJobManager = recurringJobManager;
            _logger = logger;
            _notificationJob = notificationJob;
            _emailSendingJob = emailSendingJob;
            _emailTemplateRendererService = emailTemplateRendererService;
        }

        public async Task ScheduleCancelNotConfirmedBookingAfterNDaysAsync(int bookingId, DateTime cancelDate, string message)
        {
            string jobId = $"cancel-not-confirmed-booking-{bookingId}";

            BackgroundJob.Schedule(() => CancelBookingAsync(bookingId, jobId, message, ReScheduleCancelNotConfirmedBookingAfterNDays), cancelDate);
        }

        public void ReScheduleCancelNotConfirmedBookingAfterNDays(int bookingId, string jobId, string message)
        {
            _recurringJobManager.AddOrUpdate(jobId, () => CancelBookingAsync(bookingId, jobId, message, ReScheduleCancelNotConfirmedBookingAfterNDays), Cron.Daily);
        }

        public async Task ScheduleCancelNotResponsedBookingAsync(int bookingId, DateTime cancelDate, string message)
        {
            string jobId = $"cancel-not-responsed-booking-{bookingId}";

            BackgroundJob.Schedule(() => CancelBookingAsync(bookingId, jobId, message, ReScheduleCancelNotResponsedBooking), cancelDate);
        }

        public void ReScheduleCancelNotResponsedBooking(int bookingId, string jobId, string message)
        {
            _recurringJobManager.AddOrUpdate(jobId, () => CancelBookingAsync(bookingId, jobId, message, ReScheduleCancelNotResponsedBooking), Cron.Daily);
        }

        public async Task CancelBookingAsync(int bookingId, string jobId, string message, Action<int, string, string> rescheduledJob)
        {
            Booking? booking = await _unitOfWork.Bookings.FindAsync(b => b.Id == bookingId, includes: [BookingIncludes.Customer, BookingIncludes.Vehicle, $"{BookingIncludes.Vehicle}.{VehicleIncludes.Renter}", ]);
            if(booking == null)
            {
                _logger.LogWarning($"Booking {bookingId} is not found");
                return;
            }

            if(booking.Status != DATA.Models.Enums.BookingStatus.Pending && booking.Status != DATA.Models.Enums.BookingStatus.Accepted)
            {
                _logger.LogWarning($"Booking {bookingId} can't be canceled");
                return;
            }

            booking.Status = DATA.Models.Enums.BookingStatus.Canceled;
            await _unitOfWork.Bookings.AddOrUpdateAsync(booking);
            int changes = await _unitOfWork.CommitAsync();

            if(changes == 0)
            {
                _logger.LogWarning($"Failed to cancel Booking {bookingId}, try again tommorow");
                rescheduledJob(bookingId, jobId, message); // avoid duplication
                return;
            }

            _logger.LogInformation($"Canceled booking {bookingId} successfuly");

            // notify renter
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = message,
                ReceiverId = booking.Vehicle.RenterId,
                TargetId = booking.Id,
                TargetType = DATA.Models.Enums.TargetType.Booking
            });

            // notify customer
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = message,
                ReceiverId = booking.CustomerId,
                TargetId = booking.Id,
                TargetType = DATA.Models.Enums.TargetType.Booking
            });

            _emailSendingJob.SendEmail(booking.Customer.Email, EmailSubject.NotificationReceived, await _emailTemplateRendererService.RenderTemplateAsync(Templates.Notification, new { NotificationContent = System.Web.HttpUtility.HtmlEncode(message), NotificationType = System.Web.HttpUtility.HtmlEncode(NotificationType.BookingCanceled) }));

            _recurringJobManager.RemoveIfExists(jobId);
        }
    }
}
