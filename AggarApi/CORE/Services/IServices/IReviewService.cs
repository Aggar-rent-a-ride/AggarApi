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
        Task<ResponseDto<IEnumerable<SummarizedReviewDto>>> GetUserReviewsAsync(int userId, int pageNo, int pageSize);
        Task<ResponseDto<GetReviewDto>> GetReviewAsync(int reviewId);
    }
}
