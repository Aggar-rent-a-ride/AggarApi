using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Rental;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class RentalService : IRentalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RentalService> _logger;

        public RentalService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RentalService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto<GetRentalDto?>> GetRentalByIdAsync(int rentalId)
        {
            _logger.LogInformation("Getting rental with ID: {RentalId}", rentalId);

            var rental = await _unitOfWork.Rentals.GetAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning("Rental with ID: {RentalId} not found", rentalId);
                return new ResponseDto<GetRentalDto?>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Rental not found."
                };
            }

            _logger.LogInformation("Successfully retrieved rental with ID: {RentalId}", rentalId);
            return new ResponseDto<GetRentalDto?>
            {
                StatusCode = StatusCodes.OK,
                Data = _mapper.Map<GetRentalDto>(rental)
            };
        }

        public async Task<ResponseDto<IEnumerable<GetRentalsByUserIdDto>>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize, int maxPageSize = 100)
        {
            _logger.LogInformation("Getting rentals for user with ID: {UserId}", userId);

            if(PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {PaginationError}", paginationError);
                return new ResponseDto<IEnumerable<GetRentalsByUserIdDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }

            var rentals = await _unitOfWork.Rentals.GetRentalsByUserIdAsync(userId, pageNo, pageSize);

            _logger.LogInformation("Successfully retrieved rentals for user with ID: {UserId}", userId);
            return new ResponseDto<IEnumerable<GetRentalsByUserIdDto>>
            {
                StatusCode = StatusCodes.OK,
                Data = _mapper.Map<IEnumerable<GetRentalsByUserIdDto>>(rentals)
            };
        }

        public async Task<ResponseDto<IEnumerable<GetRentalsByVehicleIdDto>>> GetRentalsByVehicleIdAsync(int vehicleId, int pageNo, int pageSize, int maxPageSize = 100)
        {
            _logger.LogInformation("Getting rentals for vehicle with ID: {VehicleId}", vehicleId);

            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {PaginationError}", paginationError);
                return new ResponseDto<IEnumerable<GetRentalsByVehicleIdDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }

            var rentals = await _unitOfWork.Rentals.GetRentalsByVehicleIdAsync(vehicleId, pageNo, pageSize);

            _logger.LogInformation("Successfully retrieved rentals for vehicle with ID: {VehicleId}", vehicleId);
            return new ResponseDto<IEnumerable<GetRentalsByVehicleIdDto>>
            {
                StatusCode = StatusCodes.OK,
                Data = _mapper.Map<IEnumerable<GetRentalsByVehicleIdDto>>(rentals)
            };
        }

        public async Task<ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?>> GetReviewRentalValidationProperties(int rentalId)
        {
            _logger.LogInformation("Getting rental with ID: {RentalId}", rentalId);

            var rental = await _unitOfWork.Rentals.GetRentalByIdIncludingBookingThenIncludingVehicleAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning("Rental with ID: {RentalId} not found", rentalId);
                return new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Rental not found."
                };
            }

            _logger.LogInformation("Successfully retrieved rental with ID: {RentalId}", rentalId);
            return new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?>
            {
                StatusCode = StatusCodes.OK,
                Data = rental
            };
        }

        private string GetRentalStatus(Booking booking)
        {
            if (booking.Status == DATA.Models.Enums.BookingStatus.Canceled)
                return RentalStatus.Cancelled;

            // accepted
            if (booking.StartDate > DateTime.UtcNow)
                return RentalStatus.NotStarted;

            if (booking.StartDate <= DateTime.UtcNow && booking.EndDate >= DateTime.UtcNow)
                return RentalStatus.InProgress;

            return RentalStatus.Completed;
        }
        public async Task<ResponseDto<IEnumerable<RentalHistoryItemDto>>> GetUserRentalHistoryAsync(int userId, int pageNo, int pageSize, int maxPageSize = 50)
        {
            _logger.LogInformation("Getting rental history for user with ID: {UserId}", userId);
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {PaginationError}", paginationError);
                return new ResponseDto<IEnumerable<RentalHistoryItemDto>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }
            
            var rentals = await _unitOfWork.Rentals.GetUserRentalHistoryAsync(userId, pageNo, pageSize);

            _logger.LogInformation("Successfully retrieved rental history for user with ID: {UserId}", userId);

            var result = new List<RentalHistoryItemDto>();
            foreach (var rental in rentals)
            {
                var rentalHistoryItem = new RentalHistoryItemDto
                {
                    Id = rental.Id,
                    StartDate = rental.Booking.StartDate,
                    EndDate = rental.Booking.EndDate,
                    TotalDays = rental.Booking.TotalDays,
                    FinalPrice = rental.Booking.FinalPrice,
                    RentalStatus = GetRentalStatus(rental.Booking),
                    Discount = rental.Booking.Discount,
                    RenterReview = rental.RenterReview != null ? new RentalHistoryItemDto.ReviewDetails
                    {
                        Id = rental.RenterReview.Id,
                        RentalId = rental.RenterReview.RentalId,
                        CreatedAt = rental.RenterReview.CreatedAt,
                        Behavior = rental.RenterReview.Behavior,
                        Punctuality = rental.RenterReview.Punctuality,
                        Care = rental.RenterReview.Care,
                        Comments = rental.RenterReview.Comments,
                        Reviewer = _mapper.Map<RentalHistoryItemDto.UserDetails>(rental.Booking.Vehicle.Renter),
                    } : null,
                    CustomerReview = rental.CustomerReview != null ? new RentalHistoryItemDto.ReviewDetails
                    {
                        Id = rental.CustomerReview.Id,
                        RentalId = rental.CustomerReview.RentalId,
                        CreatedAt = rental.CustomerReview.CreatedAt,
                        Behavior = rental.CustomerReview.Behavior,
                        Punctuality = rental.CustomerReview.Punctuality,
                        Truthfulness = rental.CustomerReview.Truthfulness,
                        Comments = rental.CustomerReview.Comments,
                        Reviewer = _mapper.Map<RentalHistoryItemDto.UserDetails>(rental.Booking.Customer),
                    } : null,
                    Vehicle = _mapper.Map<RentalHistoryItemDto.VehicleDetails>(rental.Booking.Vehicle),
                    User = userId == rental.Booking.Customer.Id
                        ? _mapper.Map<RentalHistoryItemDto.UserDetails>(rental.Booking.Vehicle.Renter)
                        : _mapper.Map<RentalHistoryItemDto.UserDetails>(rental.Booking.Customer)
                };
                result.Add(rentalHistoryItem);
            }

            return new ResponseDto<IEnumerable<RentalHistoryItemDto>>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }
    }
}
