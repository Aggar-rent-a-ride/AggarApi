using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.BackgroundJobs.IBackgroundJobs;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Triangulate;

namespace CORE.BackgroundJobs
{
    public class VehiclePopularityManagementJob : IVehiclePopularityManagementJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehiclePopularityManagementJob> _logger;
        private readonly IMemoryCache _memoryCache;
        public VehiclePopularityManagementJob(IUnitOfWork unitOfWork, ILogger<VehiclePopularityManagementJob> logger, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _memoryCache = memoryCache;
        }
        private string GenerateCacheKey(int vehicleId, int userId)
        {
            return $"VehiclePopularity_{vehicleId}_{userId}";
        }
        public void Execute(int vehicleId, int userId)
        {
            BackgroundJob.Enqueue(() => ProcessVehiclePopularityAsync(vehicleId, userId));
        }
        public async Task ProcessVehiclePopularityAsync(int vehicleId, int userId)
        {
            if (ProcessCache(vehicleId, userId) == false)
            {
                _logger.LogInformation($"VehiclePopularity for VehicleId {vehicleId} and UserId {userId} has already been processed.");
                return;
            }
            await IncreaseVehiclePopularityPoints(vehicleId);
        }
        private bool ProcessCache(int vehicleId, int userId)
        {
            var key = GenerateCacheKey(vehicleId, userId);
            if (_memoryCache.TryGetValue(key, out bool isProcessed) == true)
            {
                _logger.LogInformation($"VehiclePopularity for VehicleId {vehicleId} and UserId {userId} has already been processed.");
                return false;
            }
            _memoryCache.Set(key, true, TimeSpan.FromDays(1));
            return true;
        }
        private async Task IncreaseVehiclePopularityPoints(int vehicleId)
        {
            var vehiclePopularity = await _unitOfWork.VehiclePopularity.FindAsync(vp => vp.VehicleId == vehicleId);
            if (vehiclePopularity == null)
            {
                _logger.LogWarning($"VehiclePopularity with VehicleId {vehicleId} not found.");

                _logger.LogInformation($"Creating new VehiclePopularity for VehicleId {vehicleId}.");

                var newVehiclePopularity = new VehiclePopularity
                {
                    VehicleId = vehicleId,
                    PopularityPoints = 1
                };
                await _unitOfWork.VehiclePopularity.AddOrUpdateAsync(newVehiclePopularity);
            }
            else
                vehiclePopularity.PopularityPoints++;

            var changes = await _unitOfWork.CommitAsync();
            if(changes == 0)
                _logger.LogWarning($"No changes were made to VehiclePopularity with VehicleId {vehicleId}.");
            else
                _logger.LogInformation($"VehiclePopularity points for VehicleId {vehicleId} increased.");
        }
    }
}
