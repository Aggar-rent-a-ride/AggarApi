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
        Task<ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?>> GetReviewRentalValidationProperties(int rentalId);
        Task<ResponseDto<IEnumerable<GetRentalsByUserIdDto>>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<IEnumerable<GetRentalsByUserIdDto>>> GetRentalsByUserIdAsync(int userId);
        Task<ResponseDto<IEnumerable<GetRentalsByVehicleIdDto>>> GetRentalsByVehicleIdAsync(int vehicleId, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<PagedResultDto<IEnumerable<RentalHistoryItemDto>>>> GetUserRentalHistoryAsync(int userId, int pageNo, int pageSize, int maxPageSize = 50);
        Task<CreatedRentalDto> CreateRentalAsync(Booking booking);
        Task<ResponseDto<object>> ConfirmRentalAsync(int customerId, int rentalId, string receivedQrCodeToken);
        Task HandleTransferSucceededAsync(int rentalId);
        Task HandleTransferFailedAsync(int rentalId);
        Task<ResponseDto<object>> RefundRentalAsync(int customerId, int rentalId);
        Task HandleRefundSucceededAsync(int rentalId);
        Task HandleRefundFailedAsync(int rentalId);
    }
}
