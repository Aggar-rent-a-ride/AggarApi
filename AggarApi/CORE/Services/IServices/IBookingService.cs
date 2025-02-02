using CORE.DTOs;
using CORE.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IBookingService
    {
        public Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime startDate, DateTime endDate);
        public Task<ResponseDto<CreateBookingDto>> CreateBookingAsync(CreateBookingDto createBookingDto, int userId);
        public Task<ResponseDto<BookingDetailsDto>> GetBookingAsync(int bookingId);
        //public Task<ResponseDto<>> ConfirmBookingAsync(int bookingId);
        //public Task<ResponseDto<>> CancelBookingAsync(int bookingId);
    }
}
