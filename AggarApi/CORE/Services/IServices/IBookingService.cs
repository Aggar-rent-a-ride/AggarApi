using CORE.DTOs;
using CORE.DTOs.Booking;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IBookingService
    {
        public Task<ResponseDto<BookingDetailsDto>> CreateBookingAsync(CreateBookingDto createBookingDto, int customerId);
        public Task<bool> CheckVehicleAvailability(int vehicleId, DateTime startDate, DateTime endDate);
        public Task<ResponseDto<BookingDetailsDto>> GetBookingByIdAsync(int bookingId, int? userId);
        //public Task<ResponseDto<>> ConfirmBookingAsync(int bookingId);
        public Task<ResponseDto<object>> CancelBookingAsync(int bookingId, int? customerId);
        public Task<ResponseDto<object>> ResponseBookingRequestAsync(int bookingId, int? renterId, bool isAccepted);
        //public Task<ResponseDto<PagedResultDto<BookingSummaryDto>>> GetBookingAsync(int userId, int pageIndex, int pageSize); // maybe in profile
        Task<ResponseDto<BookingDetailsDto>> GetBookingByRentalIdAsync(int rentalId);
    }
}
