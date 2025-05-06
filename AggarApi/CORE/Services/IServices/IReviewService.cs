using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IReviewService
    {
        Task<ResponseDto<GetReviewDto>> CreateReviewAsync(CreateReviewDto reviewDto, int userId, string role);
        Task<ResponseDto<GetReviewDto>> GetReviewAsync(int reviewId);
        Task<ResponseDto<IEnumerable<SummarizedReviewDto>>> GetVehicleReviewsAsync(int vehicleId, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<double?>> GetVehicleTotalRateAsync(int vehicleId);
    }
}
