﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Review;
using CORE.DTOs;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants.Includes;
using CORE.Constants;
using Microsoft.Extensions.Logging;
using DATA.DataAccess.Repositories.UnitOfWork;
using AutoMapper;
using CORE.DTOs.AppUser;

namespace CORE.Services
{
    public class VehicleReviewService : IVehicleReviewService
    {
        private readonly ILogger<VehicleReviewService> _logger;
        private readonly IRentalService _rentalService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleReviewService(ILogger<VehicleReviewService> logger, IRentalService rentalService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _rentalService = rentalService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>> GetVehicleReviewsAsync(int vehicleId, int pageNo, int pageSize, int maxPageSize = 100)
        {
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {ErrorMessage}", paginationError);
                return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }

            var vehicleRentalsResponse = await _rentalService.GetRentalsByVehicleIdAsync(vehicleId, 1, maxPageSize, maxPageSize);
            if (vehicleRentalsResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to retrieve rentals for vehicle {VehicleId}: {ErrorMessage}",
                    vehicleId, vehicleRentalsResponse.Message);
                return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
                {
                    StatusCode = vehicleRentalsResponse.StatusCode,
                    Message = vehicleRentalsResponse.Message
                };
            }

            var rentals = vehicleRentalsResponse.Data;
            if (rentals == null || rentals.Any() == false)
            {
                var emptyResult = new List<SummarizedReviewDto>();
                _logger.LogInformation("No rentals found for vehicle {VehicleId}", vehicleId);
                return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
                {
                    StatusCode = StatusCodes.OK,
                    Message = "No rentals found for this vehicle",
                    Data = PaginationHelpers.CreatePagedResult(emptyResult.AsEnumerable(), pageNo, pageSize, 0)
                };
            }

            rentals = rentals.OrderBy(r => r.Id);


            var customerReviewsIds = rentals.Where(r => r.CustomerReviewId != 0).Select(r => r.CustomerReviewId);
            customerReviewsIds = customerReviewsIds.OrderByDescending(r => r).Skip((pageNo - 1) * pageSize).Take(pageSize);

            var includes = new List<string> { CustomerReviewIncludes.Customer };
            var reviews = await _unitOfWork.CustomerReviews.FindAsync(r => customerReviewsIds.Contains(r.Id), 1, maxPageSize, includes.ToArray());
            var count = await _unitOfWork.Vehicles.GetVehicleReviewsCountAsync(vehicleId);
            var result = _mapper.Map<IEnumerable<SummarizedReviewDto>>(reviews).ToList();

            _logger.LogInformation("Successfully retrieved reviews for vehicle {VehicleId}", vehicleId);
            return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(result.AsEnumerable(), pageNo, pageSize, count)
            };
        }

        public async Task<ResponseDto<double?>> GetVehicleTotalRateAsync(int vehicleId)
        {
            _logger.LogInformation("Received request to get total rate for vehicle {VehicleId}", vehicleId);

            var vehicleReviewsResponse = await GetVehicleReviewsAsync(vehicleId, 1, Int32.MaxValue, Int32.MaxValue);

            if (vehicleReviewsResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to retrieve reviews for vehicle {VehicleId}: {ErrorMessage}",
                    vehicleId, vehicleReviewsResponse.Message);
                return new ResponseDto<double?>
                {
                    StatusCode = vehicleReviewsResponse.StatusCode,
                    Message = vehicleReviewsResponse.Message
                };
            }
            var reviews = vehicleReviewsResponse.Data.Data;

            var totalRate = Math.Round(reviews.Average(r => r.Rate), 1);

            _logger.LogInformation("Successfully retrieved total rate for vehicle {VehicleId}: {TotalRate}",
                vehicleId, totalRate);
            return new ResponseDto<double?>
            {
                StatusCode = StatusCodes.OK,
                Data = totalRate
            };
        }
    }
}
