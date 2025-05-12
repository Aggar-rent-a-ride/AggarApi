using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Review;
using CORE.DTOs;

namespace CORE.Services.IServices
{
    public interface IVehicleReviewService
    {
        Task<ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>> GetVehicleReviewsAsync(int vehicleId, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<double?>> GetVehicleTotalRateAsync(int vehicleId);
    }
}
