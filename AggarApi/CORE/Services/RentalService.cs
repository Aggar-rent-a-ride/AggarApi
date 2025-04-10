using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Rental;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    StatusCode = StatusCodes.NotFound,
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
            if (rentals == null || rentals.Any() == false)
            {
                _logger.LogWarning("No rentals found for user with ID: {UserId}", userId);
                return new ResponseDto<IEnumerable<GetRentalsByUserIdDto>>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "No rentals found."
                };
            }

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
            if (rentals == null || rentals.Any() == false)
            {
                _logger.LogWarning("No rentals found for vehicle with ID: {VehicleId}", vehicleId);
                return new ResponseDto<IEnumerable<GetRentalsByVehicleIdDto>>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "No rentals found."
                };
            }

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
                    StatusCode = StatusCodes.NotFound,
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
    }
}
