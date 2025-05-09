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
using DATA.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> CheckVehicleAvailability(int vehicleId, DateTime startDate, DateTime endDate)
        {
            bool result = await _unitOfWork.Bookings.Where(b => b.VehicleId == vehicleId && 
            (b.Status != BookingStatus.Canceled && b.Status != BookingStatus.Rejected))
                .AnyAsync(b => b.StartDate <= endDate && b.EndDate >= startDate);

            return ! result;
        }

        public async Task<ResponseDto<BookingDetailsDto>> CreateBookingAsync(CreateBookingDto createBookingDto, int customerId)
        {
            if (customerId == 0)
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Customer Id is required",
                    StatusCode = StatusCodes.Unauthorized
                };

            Vehicle? vehicle = await _unitOfWork.Vehicles.FindAsync(v => v.Id == createBookingDto.VehicleId, [VehicleIncludes.Bookings]);

            if (createBookingDto.StartDate < DateTime.UtcNow ||
                createBookingDto.StartDate > createBookingDto.EndDate)
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Date is not valid",
                    StatusCode = StatusCodes.BadRequest
                };

            if (vehicle == null)
            {
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Vehicle not found",
                    StatusCode = StatusCodes.BadRequest,
                };
            }
            else if (vehicle.Status != VehicleStatus.Active)
            {
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Vehicle is out of service",
                    StatusCode = StatusCodes.Conflict,
                };
            }
            else if (await CheckVehicleAvailability(vehicle.Id, createBookingDto.StartDate, createBookingDto.EndDate) == false)
            {
                return new ResponseDto<BookingDetailsDto>
                {
                    Message = "Vehicle is not available in this interval",
                    StatusCode = StatusCodes.Conflict,
                };
            }

            Booking newBooking = _mapper.Map<Booking>(createBookingDto);

            newBooking.CustomerId = customerId;
            newBooking.Price = vehicle.PricePerDay * createBookingDto.EndDate.TotalDays(createBookingDto.StartDate);
            
            Discount? discount = vehicle.Discounts?.
                Where(d => d.DaysRequired <= newBooking.TotalDays)
                .OrderByDescending(d => d.DiscountPercentage)
                .FirstOrDefault();

            if(discount != null)
            {
                newBooking.Discount = discount.DiscountPercentage;
            }
            else
            {
                newBooking.Discount = 0;
            }

            await _unitOfWork.Bookings.AddOrUpdateAsync(newBooking);

            int changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Faild to save booking"
                };

            var addedBookingResult = await GetBookingByIdAsync(newBooking.Id, customerId);
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
            
        public async Task<ResponseDto<BookingDetailsDto>> GetBookingByIdAsync(int bookingId, int userId)
        {
            if (userId <= 0)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "UserId Not Found"
                };

            Booking? booking = await _unitOfWork.Bookings.FindAsync(b => b.Id == bookingId, [BookingIncludes.Vehicle]);
            if (booking.CustomerId != userId && booking.Vehicle.RenterId != userId)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found" // authorization
                };

            if (booking == null)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found"
                };

            BookingDetailsDto bookingDetailsDto = _mapper.Map<BookingDetailsDto>(booking);
            bookingDetailsDto.VehicleImagePath = booking.Vehicle.MainImagePath;
            bookingDetailsDto.VehicleModel = booking.Vehicle.Model;
            bookingDetailsDto.VehicleYear = booking.Vehicle.Year;
            bookingDetailsDto.VehicleBrand = booking.Vehicle.VehicleBrand?.Name;
            bookingDetailsDto.VehicleType = booking.Vehicle.VehicleType?.Name;

            return new ResponseDto<BookingDetailsDto>
            {
                StatusCode = StatusCodes.OK,
                Data = bookingDetailsDto,
                Message = "Booking uploaded successfuly"
            };
        }

        public async Task<ResponseDto<object>> CancelBookingAsync(int bookingId, int customerId)
        {
            if (customerId == 0)
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

            if(booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Accepted)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Conflict,
                    Message = "You can't Cancel This Booking"
                };
            }

            booking.Status = BookingStatus.Canceled;
            // remove

            // notify renter

            int changes = await _unitOfWork.CommitAsync();
            if (changes > 0)
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

        public async Task<ResponseDto<object>> ResponseBookingRequestAsync(int bookingId, int renterId, bool isAccepted)
        {
            if (renterId == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Renter Id is required"
                };

            Booking? booking = await _unitOfWork.Bookings.FindAsync(b => b.Id == bookingId, [BookingIncludes.Vehicle]);
            if (booking == null || booking.Vehicle.RenterId != renterId)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found"
                };

            if(booking.Status != BookingStatus.Pending)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.Conflict,
                    Message = "You can't Response This Booking"
                };
            }

            if (isAccepted)
            {
                booking.Status = BookingStatus.Accepted;
            }
            else
            {
                booking.Status = BookingStatus.Rejected;
                // remove
            }

            // notify customer

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

        public async Task<ResponseDto<BookingDetailsDto>> GetBookingByRentalIdAsync(int rentalId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingByRentalIdAsync(rentalId);
            if (booking == null)
                return new ResponseDto<BookingDetailsDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Booking not found"
                };

            return new ResponseDto<BookingDetailsDto>
            {
                StatusCode = StatusCodes.OK,
                Data = _mapper.Map<BookingDetailsDto>(booking),
            };
        }
    }
}
