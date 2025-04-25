using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Review;
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
    public class RentalReviewService : IRentalReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RentalReviewService> _logger;

        public RentalReviewService(IUnitOfWork unitOfWork, ILogger<RentalReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResponseDto<object>> CreateReviewUpdateRentalAsync(Review? review, string reviewerRole, int rentalId)
        {
            if(review == null)
            {
                _logger.LogError("Review is null");
                return new ResponseDto<object>
                {
                    StatusCode = 400,
                    Message = "Review cannot be null",
                };
            }

            var rental = await _unitOfWork.Rentals.GetAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning("Rental with ID: {RentalId} not found", rentalId);
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Rental not found."
                };
            }

            await _unitOfWork.BeginTransactionAsync();
            if (reviewerRole == Roles.Customer)
                await _unitOfWork.CustomerReviews.AddOrUpdateAsync(review as CustomerReview);
            else
                await _unitOfWork.RenterReviews.AddOrUpdateAsync(review as RenterReview);

            await _unitOfWork.CommitAsync();

            if (reviewerRole == Roles.Customer)
                rental.CustomerReviewId = review.Id;
            else
                rental.RenterReviewId = review.Id;

            await _unitOfWork.CommitAsync();


            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully updated rental review ID: {ReviewId} for rental ID: {RentalId}", review.Id, rentalId);
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "Rental review updated successfully."
            };
        }
    }
}
