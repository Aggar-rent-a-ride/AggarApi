using CORE.DTOs;
using CORE.DTOs.Rental;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IRentalService
    {
        Task<ResponseDto<GetRentalDto?>> GetRentalByIdAsync(int rentalId);
        Task<ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?>> GetReviewRentalValidationProperties(int rentalId);
        Task<ResponseDto<IEnumerable<GetRentalsByUserIdDto>>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize);
    }
}
