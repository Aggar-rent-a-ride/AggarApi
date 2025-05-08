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
    public class VehicleRatingUpdateJob
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleRatingUpdateJob> _logger;
        private readonly IVehicleReviewService _vehicleReviewService;

        public VehicleRatingUpdateJob(IRecurringJobManager recurringJobManager, IUnitOfWork unitOfWork, ILogger<VehicleRatingUpdateJob> logger, IVehicleReviewService vehicleReviewService)
        {
            _recurringJobManager = recurringJobManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _vehicleReviewService = vehicleReviewService;
        }
        public void ScheduleVehicleRatingUpdate(int vehicleId)
        {
            string jobId = $"update-vehicle-rating-{vehicleId}";

            // Schedule to run immediately
            BackgroundJob.Enqueue(() => ProcessVehicleRatingUpdate(vehicleId, jobId));
        }
        private void RescheduleProcessVehicleRatingUpdate(string jobId, int vehicleId)
        {
            // Reschedule for tomorrow
            _recurringJobManager.AddOrUpdate(jobId,
                () => ProcessVehicleRatingUpdate(vehicleId, jobId),
                Cron.Daily());
        }
        public async Task ProcessVehicleRatingUpdate(int vehicleId, string jobId)
        {
            try
            {
                var rateResponse = await _vehicleReviewService.GetVehicleTotalRateAsync(vehicleId);
                if (rateResponse.StatusCode != StatusCodes.OK)
                {
                    _logger.LogWarning($"Failed to get rates for vehicle {vehicleId} - scheduling retry for tomorrow");
                    RescheduleProcessVehicleRatingUpdate(jobId, vehicleId);
                    return;
                }

                // Update vehicle rating
                var updateResponse = await UpdateVehicleRatingAsync(vehicleId, rateResponse.Data.Value);

                if (updateResponse.StatusCode != StatusCodes.OK)
                {
                    _logger.LogWarning($"Failed to update rating for vehicle {vehicleId} - scheduling retry for tomorrow");
                    RescheduleProcessVehicleRatingUpdate(jobId, vehicleId);
                    return;
                }

                _logger.LogInformation($"Successfully updated rating for vehicle {vehicleId} to {rateResponse.Data}");
                _recurringJobManager.RemoveIfExists(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing rating update for vehicle {vehicleId} - scheduling retry for tomorrow");
                RescheduleProcessVehicleRatingUpdate(jobId, vehicleId);
            }
        }

        private async Task<ResponseDto<object>> UpdateVehicleRatingAsync(int vehicleId, double rate)
        {
            var vehicle = await _unitOfWork.Vehicles.GetAsync(vehicleId);
            if (vehicle == null)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Vehicle not found"
                };
            }
            vehicle.Rate = rate;
            var changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed to update vehicle rating"
                };

            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "Vehicle rating updated successfully"
            };
        }

    }
}
