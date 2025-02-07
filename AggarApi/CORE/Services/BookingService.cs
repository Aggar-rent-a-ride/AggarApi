using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Booking;
using CORE.DTOs.Vehicle;
using CORE.Extensions;
using CORE.Services.IServices;
using DATA.Constants.Includes;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMapper _mapper;
        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        private bool CheckVehicleAvailability(Vehicle vehicle, DateTime startDate, DateTime endDate)
        {
            bool result = vehicle.Bookings.Where(b => b.VehicleId == vehicle.Id &&
            (b.Status == DATA.Models.Enums.BookingStatus.Accepted || b.Status == DATA.Models.Enums.BookingStatus.Accepted))
                .All(b => b.StartDate > endDate || b.EndDate < startDate);
            
            return result;
        }



        public async Task<ResponseDto<BookingDetailsDto>> CreateBookingAsync(CreateBookingDto createBookingDto, int? customerId)
        {
            if (customerId.Value == 0 || !customerId.HasValue)
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Customer Id is required",
                    StatusCode = StatusCodes.InternalServerError,
                    Data = null
                };

            Vehicle? vehicle = await _unitOfWork.Vehicles.GetAsync(createBookingDto.VehicleId, [VehicleIncludes.Bookings]);

            if (createBookingDto.StartDate < DateTime.Now ||
                createBookingDto.StartDate < DateTime.Now ||
                createBookingDto.StartDate > createBookingDto.EndDate)
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Date is not valid",
                    StatusCode = StatusCodes.BadRequest,
                    Data = null
                };

            if (vehicle == null)
            {
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Vehicle not found",
                    StatusCode = StatusCodes.BadRequest,
                    Data = null
                };
            }
            else if (vehicle.Status != DATA.Models.Enums.VehicleStatus.Active)
            {
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Vehicle is out of service",
                    StatusCode = StatusCodes.OK,
                    Data = null
                };
            }
            else if (!CheckVehicleAvailability(vehicle, createBookingDto.StartDate, createBookingDto.EndDate))
            {
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Vehicle is not available in this interval",
                    StatusCode = StatusCodes.OK,
                    Data = null
                };
            }

            // discount
            Booking newBooking = _mapper.Map<Booking>(createBookingDto);
            newBooking.Price = vehicle.PricePerDay * createBookingDto.EndDate.TotalDays(createBookingDto.StartDate);

            await _unitOfWork.Bookings.AddOrUpdateAsync(newBooking);

            int changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Faild to save booking"
                };

            var addedBookingResult = await GetBookingByIdAsync(newBooking.Id, customerId.Value);
            if (addedBookingResult.StatusCode != StatusCodes.OK)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Booking added but failed to retrieve it"
                };
            return new ResponseDto<BookingDetailsDto>
            {
                StatusCode = StatusCodes.Created,
                Message = "Booking added successfully",
                Data = addedBookingResult.Data
            };
        }
            
        public async Task<ResponseDto<BookingDetailsDto>> GetBookingByIdAsync(int bookingId, int? userId)
        {
            if (!userId.HasValue)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "UserId is required"
                };

            Booking? booking = await _unitOfWork.Bookings.GetAsync(bookingId, [BookingIncludes.Vehicle]);
            if (booking.CustomerId != userId.Value && booking.Vehicle.RenterId != userId.Value)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.Unauthorized,
                    Message = "Not Allowed"
                };

            if (booking == null)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found"
                };

            BookingDetailsDto bookingDetailsDto = _mapper.Map<BookingDetailsDto>(booking);

            return new ResponseDto<BookingDetailsDto>
            {
                StatusCode = StatusCodes.OK,
                Data = bookingDetailsDto,
                Message = "Booking uploaded successfuly"
            };
        }

        public async Task<ResponseDto<object>> CancelBookingAsync(int bookingId, int? customerId)
        {
            if (!customerId.HasValue)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "customer Id is required"
                };

            Booking? booking = await _unitOfWork.Bookings.GetAsync(bookingId);
            if (booking == null)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found"
                };

            booking.Status = DATA.Models.Enums.BookingStatus.Canceled;
            // remove
            int changes = await _unitOfWork.CommitAsync();
            if(changes > 0)
                return new ResponseDto<object>
                {
                StatusCode = StatusCodes.OK,
                Message = "Booking canceled successfuly"
                };
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.InternalServerError,
                Message = "Faild to cancel booking"
            };
        }

        public async Task<ResponseDto<object>> ResponseBookingRequestAsync(int bookingId, int? renterId, bool isAccepted)
        {
            if (!renterId.HasValue)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Renter Id is required"
                };

            Booking? booking = await _unitOfWork.Bookings.GetAsync(bookingId);
            if (booking == null)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found"
                };

            if(isAccepted)
                booking.Status = DATA.Models.Enums.BookingStatus.Accepted;
            else
                booking.Status = DATA.Models.Enums.BookingStatus.Rejected;
            // remove
            int changes = await _unitOfWork.CommitAsync();
            if (changes > 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.OK,
                    Message = "Booking responsed successfuly"
                };
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.InternalServerError,
                Message = "Faild to response booking"
            };
        }
    }
}
