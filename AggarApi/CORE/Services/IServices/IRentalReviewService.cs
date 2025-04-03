using CORE.DTOs;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IRentalReviewService
    {
        Task<ResponseDto<object>> CreateReviewUpdateRentalAsync(Review? review, string reviewerRole, int rentalId);
    }
}
