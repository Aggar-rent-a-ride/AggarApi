using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Review;
using CORE.DTOs;

namespace CORE.Services.IServices
{
    public interface IUserReviewService
    {
        Task<ResponseDto<IEnumerable<SummarizedReviewDto>>> GetUserReviewsAsync(int userId, int pageNo, int pageSize, int maxPageSize = 100);
    }
}
