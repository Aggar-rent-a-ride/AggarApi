using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.BackgroundJobs.IBackgroundJobs;
using DATA.DataAccess.Repositories.UnitOfWork;
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
    }
}
