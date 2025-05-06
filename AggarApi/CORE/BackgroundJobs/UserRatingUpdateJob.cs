using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.Constants;
using CORE.DTOs;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CORE.BackgroundJobs
{
    public class UserRatingUpdateJob
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserRatingUpdateJob> _logger;
        private readonly IUserReviewService _userReviewService;

        public UserRatingUpdateJob(IRecurringJobManager recurringJobManager, IUnitOfWork unitOfWork, ILogger<UserRatingUpdateJob> logger, IUserReviewService userReviewService)
        {
            _recurringJobManager = recurringJobManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userReviewService = userReviewService;
        }

        public void ScheduleUserRatingUpdate(int userId)
        {
            string jobId = $"update-user-rating-{userId}";

            // Schedule to run immediately
            BackgroundJob.Enqueue(() => ProcessUserRatingUpdate(userId, jobId));
        }
        private void RescheduleProcessUserRatingUpdate(string jobId, int userId)
        {
            // Reschedule for tomorrow
            _recurringJobManager.AddOrUpdate(jobId,
                () => ProcessUserRatingUpdate(userId, jobId),
                Cron.Daily());
        }
        public async Task ProcessUserRatingUpdate(int userId, string jobId)
        {
            try
            {
                var ratesResponse = await _userReviewService.GetAllUserRatesAsync(userId);
                if (ratesResponse.StatusCode != StatusCodes.OK)
                {
                    _logger.LogWarning($"Failed to get rates for user {userId} - scheduling retry for tomorrow");
                    RescheduleProcessUserRatingUpdate(jobId, userId);
                    return;
                }
                if (ratesResponse.Data == null || ratesResponse.Data.Count() == 0)
                {
                    _logger.LogInformation($"No rates found for user {userId} - no update needed");
                    return;
                }
                // Calculate new rating
                double newRating = ratesResponse.Data.Average();

                // Update user rating
                var updateResponse = await UpdateUserRatingAsync(userId, newRating);

                if (updateResponse.StatusCode != StatusCodes.OK)
                {
                    _logger.LogWarning($"Failed to update rating for user {userId} - scheduling retry for tomorrow");
                    RescheduleProcessUserRatingUpdate(jobId, userId);
                }

                _logger.LogInformation($"Successfully updated rating for user {userId} to {newRating}");
                _recurringJobManager.RemoveIfExists(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing rating update for user {userId} - scheduling retry for tomorrow");
                RescheduleProcessUserRatingUpdate(jobId, userId);
            }
        }

        private async Task<ResponseDto<object>> UpdateUserRatingAsync(int userId, double rate)
        {
            var user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "User not found"
                };
            }
            user.Rate = rate;
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update user rating"
                };

            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "User rating updated successfully"
            };
        }
    }
}
