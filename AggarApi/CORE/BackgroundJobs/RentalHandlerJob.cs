using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.DTOs.Payment;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.Constants.Includes;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs
{
    public class RentalHandlerJob : IRentalHandlerJob
    {
        private readonly ILogger<RentalHandlerJob> _logger;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationJob _notificationJob;
        private readonly IEmailSendingJob _emailSendingJob;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;
        private readonly IPaymentService _paymentService;
        private readonly PaymentPolicy _paymentPolicy;

        public RentalHandlerJob(IUnitOfWork unitOfWork, IRecurringJobManager recurringJobManager, ILogger<RentalHandlerJob> logger, INotificationJob notificationJob, IEmailSendingJob emailSendingJob, IEmailTemplateRendererService emailTemplateRendererService, IPaymentService paymentService, IOptions<PaymentPolicy> paymentPolicy)
        {
            _unitOfWork = unitOfWork;
            _recurringJobManager = recurringJobManager;
            _logger = logger;
            _notificationJob = notificationJob;
            _emailSendingJob = emailSendingJob;
            _emailTemplateRendererService = emailTemplateRendererService;
            _paymentService = paymentService;
            _paymentPolicy = paymentPolicy.Value;
        }

        public async Task ScheduleCancelNotConfirmedRentalAfterStartDateAsync(int rentalId, DateTime cancelDate)
        {
            string jobId = $"cancel-rental-{rentalId}";

            BackgroundJob.Schedule(() => CancelNotConfirmedBookingAfterNDaysAsync(rentalId, jobId), cancelDate);
        }

        public void ReScheduleCancelNotConfirmedRentalAfterStartDate(int rentalId, string jobId)
        {
            _recurringJobManager.AddOrUpdate(jobId, () => CancelNotConfirmedBookingAfterNDaysAsync(rentalId, jobId), Cron.Daily);
        }

        public async Task CancelNotConfirmedBookingAfterNDaysAsync(int rentalId, string jobId)
        {
            Rental? rental = await _unitOfWork.Rentals.FindAsync(r => r.Id == rentalId, includes: [RentalIncludes.Booking, $"${RentalIncludes.Booking}.{BookingIncludes.Customer}", $"{RentalIncludes.Booking}.{BookingIncludes.Vehicle}", $"{RentalIncludes.Booking}.{BookingIncludes.Vehicle}.{VehicleIncludes.Renter}"]);
            if (rental == null)
            {
                _logger.LogWarning($"Rental {rentalId} is not found");
                return;
            }

            if (rental.Status != DATA.Models.Enums.RentalStatus.NotStarted)
            {
                _logger.LogWarning($"Rental {rentalId} can't be canceled");
                return;
            }

            rental.Status = DATA.Models.Enums.RentalStatus.Canceled;
            await _unitOfWork.Rentals.AddOrUpdateAsync(rental);
            int changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                _logger.LogWarning($"Failed to cancel Rental {rentalId}, try again tommorow");
                ReScheduleCancelNotConfirmedRentalAfterStartDate(rentalId, jobId);
                return;
            }

            _logger.LogInformation($"Canceled Rental {rental} successfuly");

            // notify renter
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = "Rental has been cancelled because it exceeded the allowed days before confirmation.",
                ReceiverId = rental.Booking.Vehicle.RenterId,
                TargetId = rental.Booking.Id,
                TargetType = DATA.Models.Enums.TargetType.Rental
            });

            // notify customer
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = "Your Rental has been cancelled because it exceeded the allowed days before confirmation, Refund created...",
                ReceiverId = rental.Booking.CustomerId,
                TargetId = rental.Booking.Id,
                TargetType = DATA.Models.Enums.TargetType.Rental
            });

            _emailSendingJob.SendEmail(rental.Booking.Customer.Email, EmailSubject.NotificationReceived, await _emailTemplateRendererService.RenderTemplateAsync(Templates.Notification, new { NotificationContent = System.Web.HttpUtility.HtmlEncode("Your rental has been cancelled because it exceeded the allowed days before confirmation."), NotificationType = System.Web.HttpUtility.HtmlEncode(NotificationType.RentalCanceled) }));

            _recurringJobManager.RemoveIfExists(jobId);

            long platformFee = (long)(rental.Booking.FinalPrice * (_paymentPolicy.FeesPercentage / 100m) * 100);
            long refundedAmount = (long)(rental.Booking.FinalPrice * 100) - platformFee;
            await _paymentService.RefundAsync(rental.Booking.PaymentIntentId, rental.Booking.Vehicle.Renter.StripeAccount.StripeAccountId, rentalId, refundedAmount);
        }
    }
}
