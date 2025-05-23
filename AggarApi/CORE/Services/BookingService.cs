using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Booking;
using CORE.DTOs.Vehicle;
using CORE.Extensions;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants.Includes;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
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
        private readonly IPaymentService _paymentService;
        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _paymentService = paymentService;
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
            
            DATA.Models.Discount? discount = vehicle.Discounts?.
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

            var addedBookingResult = await GetBookingDetailsByIdAsync(newBooking.Id, customerId);
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
            
        public async Task<ResponseDto<BookingDetailsDto>> GetBookingDetailsByIdAsync(int bookingId, int userId)
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

        public async Task<ResponseDto<PagedResultDto<IEnumerable<BookingDetailsDto>>>> GetBookingsByStatusAsync(BookingStatus? status, int pageNo, int pageSize, int maxPageSize = 50)
        {
            if(PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string errorMsg)
            {
                return new ResponseDto<PagedResultDto<IEnumerable<BookingDetailsDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Invalid page number or page size"
                };
            }

            var bookings = status == null?
                await _unitOfWork.Bookings.FindAsync(b => b.Id>0, pageNo, pageSize, new string[] { BookingIncludes.Vehicle }) :
                await _unitOfWork.Bookings.FindAsync(b => b.Status == status, pageNo, pageSize, new string[] { BookingIncludes.Vehicle });
            var count = status == null ?
                await _unitOfWork.Bookings.CountAsync() :
                await _unitOfWork.Bookings.CountAsync(b => b.Status == status);

            var dtos = _mapper.Map<IEnumerable<BookingDetailsDto>>(bookings);

            return new ResponseDto<PagedResultDto<IEnumerable<BookingDetailsDto>>>
            {
                StatusCode = StatusCodes.OK,
                Message = "Bookings retrieved successfully",
                Data = PaginationHelpers.CreatePagedResult(dtos, pageNo, pageSize, count)
            };
        }

        public async Task<ResponseDto<int>> GetBookingsByStatusCountAsync(BookingStatus? status)
        {
            var count = status == null ?
                await _unitOfWork.Bookings.CountAsync() :
                await _unitOfWork.Bookings.CountAsync(b => b.Status == status);

            return new ResponseDto<int>
            {
                StatusCode = StatusCodes.OK,
                Data = count
            };
        }

        public async Task<ResponseDto<ConfirmBookingDto>> ConfirmBookingAsync(int customerId, int bookingId)
        {
            Booking? booking = await _unitOfWork.Bookings.GetAsync(bookingId);
            if (booking == null || booking.CustomerId != customerId)
            {
                return new ResponseDto<ConfirmBookingDto>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Booking not found"
                };
            }
            if (booking.Status != BookingStatus.Accepted)
            {
                return new ResponseDto<ConfirmBookingDto>
                {
                    StatusCode = StatusCodes.Conflict,
                    Message = "Not allowed to confirm booking"
                };
            }

            PaymentIntent? paymentIntent = await _paymentService.CreatePaymentIntent(booking);

            if (paymentIntent == null)
            {
                return new ResponseDto<ConfirmBookingDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "Failed To Create Payment Intent"
                };
            }

            booking.PaymentIntentId = paymentIntent.Id;

            bool res = await UpdateBookingAsync(booking);

            return new ResponseDto<ConfirmBookingDto>
            {
                StatusCode = StatusCodes.Created,
                Message = res ? "Payment Intent Created Successfuly" : "Payment Intent Created Successfuly, but failed to update Booking",
                Data = new ConfirmBookingDto
                {
                    Amount = booking.FinalPrice,
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = booking.PaymentIntentId
                }
            };

        }

        public async Task<Booking> GetBookingByIntentIdAsync(string paymentIntentId)
        {
            Booking? booking = await _unitOfWork.Bookings.FindAsync(b => b.PaymentIntentId == paymentIntentId);
            return booking;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            await _unitOfWork.Bookings.AddOrUpdateAsync(booking);
            int res = await _unitOfWork.CommitAsync();
            return res > 0;
        }
    }
}
