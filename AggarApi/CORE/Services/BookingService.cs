using CORE.DTOs;
using CORE.DTOs.Booking;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /*
        private Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
        }

        public Task<ResponseDto<CreateBookingDto>> CreateBookingAsync(CreateBookingDto createBookingDto, int userId)
        {
        }

        public Task<ResponseDto<BookingDetailsDto>> GetBookingAsync(int bookingId)
        {
        }*/
    }
}
