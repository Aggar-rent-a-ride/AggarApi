using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.Review;
using CORE.DTOs;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants.Includes;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using CORE.Constants;
using AutoMapper;

namespace CORE.Services
{
    public class UserReviewService: IUserReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserReviewService> _logger;
        private readonly IMapper _mapper;
        private readonly IRentalService _rentalService;
        public UserReviewService(IUnitOfWork unitOfWork, ILogger<UserReviewService> logger, IMapper mapper, IRentalService rentalService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _rentalService = rentalService;
        }

        public async Task<ResponseDto<IEnumerable<double>>> GetAllUserRatesAsync(int userId)
        {
            var userRentalsResponse = await _rentalService.GetRentalsByUserIdAsync(userId);
            if (userRentalsResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to retrieve rentals for user {UserId}: {ErrorMessage}",
                    userId, userRentalsResponse.Message);
                return new ResponseDto<IEnumerable<double>>
                {
                    StatusCode = userRentalsResponse.StatusCode,
                    Message = userRentalsResponse.Message
                };
            }

            var rentals = userRentalsResponse.Data;
            if (rentals == null || rentals.Any() == false)
            {
                _logger.LogInformation("No rentals found for user {UserId}", userId);
                return new ResponseDto<IEnumerable<double>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "No rentals found for this user"
                };
            }

            var result = new List<double>();
            if (rentals.First().Booking.CustomerId == userId)
            {
                //get renter reviews on that customer
                var renterReviewsIds = rentals.Select(r => r.RenterReviewId).ToHashSet();
                var reviews = await _unitOfWork.RenterReviews.GetAllRatesAsync(r => renterReviewsIds.Contains(r.Id));
                result = reviews.Select(r => (r.Punctuality + r.Care + r.Behavior)/3).ToList();
            }
            else
            {
                //get customer reviews on that renter
                var customerReviewsIds = rentals.Select(r => r.CustomerReviewId).ToHashSet();
                var reviews = await _unitOfWork.CustomerReviews.GetAllRatesAsync(r => customerReviewsIds.Contains(r.Id));
                result = reviews.Select(r => (r.Punctuality + r.Truthfulness + r.Behavior) / 3).ToList();
            }

            _logger.LogInformation("Successfully retrieved reviews for user {UserId}", userId);
            return new ResponseDto<IEnumerable<double>>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }

        public async Task<ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>> GetUserReviewsAsync(int userId, int pageNo, int pageSize, int maxPageSize = 100)
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

            var userRentalsResponse = await _rentalService.GetRentalsByUserIdAsync(userId, 1, maxPageSize, maxPageSize);
            if (userRentalsResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to retrieve rentals for user {UserId}: {ErrorMessage}",
                    userId, userRentalsResponse.Message);
                return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
                {
                    StatusCode = userRentalsResponse.StatusCode,
                    Message = userRentalsResponse.Message
                };
            }

            var rentals = userRentalsResponse.Data;
            if (rentals == null || rentals.Any() == false)
            {
                _logger.LogInformation("No rentals found for user {UserId}", userId);
                return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "No rentals found for this user"
                };
            }

            rentals = rentals.OrderBy(r => r.Id);


            var result = new List<SummarizedReviewDto>();
            var count = 0;
            if (rentals.First().Booking.CustomerId == userId)
            {

                //get renter reviews on that customer
                var renterReviewsIds = rentals.Where(r=>r.RenterReviewId != 0).Select(r => r.RenterReviewId);
                renterReviewsIds = renterReviewsIds.OrderDescending().Skip((pageNo - 1) * pageSize).Take(pageSize);

                var includes = new List<string> { RenterReviewIncludes.Renter };
                var reviews = await _unitOfWork.RenterReviews.FindAsync(r => renterReviewsIds.Contains(r.Id), 1, maxPageSize, includes.ToArray());
                result = _mapper.Map<IEnumerable<SummarizedReviewDto>>(reviews).ToList();
                count = await _unitOfWork.Rentals.GetRentalsByUserIdCountAsync(userId, Roles.Customer);
            }
            else
            {
                //get customer reviews on that renter
                var customerReviewsIds = rentals.Where(r => r.CustomerReviewId != 0).Select(r => r.CustomerReviewId);
                customerReviewsIds = customerReviewsIds.OrderByDescending(r => r).Skip((pageNo - 1) * pageSize).Take(pageSize);

                var includes = new List<string> { CustomerReviewIncludes.Customer };
                var reviews = await _unitOfWork.CustomerReviews.FindAsync(r => customerReviewsIds.Contains(r.Id), 1, maxPageSize, includes.ToArray());
                result = _mapper.Map<IEnumerable<SummarizedReviewDto>>(reviews).ToList();
                count = await _unitOfWork.Rentals.GetRentalsByUserIdCountAsync(userId, Roles.Renter);
            }

            _logger.LogInformation("Successfully retrieved reviews for user {UserId}", userId);
            return new ResponseDto<PagedResultDto<IEnumerable<SummarizedReviewDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(result.AsEnumerable(), pageNo, pageSize, count),
            };
        }

    }
}
