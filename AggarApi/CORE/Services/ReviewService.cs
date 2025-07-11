﻿using AutoMapper;
using CORE.BackgroundJobs;
using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Notification;
using CORE.DTOs.Rental;
using CORE.DTOs.Review;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.Constants.Includes;
using DATA.DataAccess.Context.Configurations;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRentalService _rentalService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;
        private readonly IRentalReviewService _rentalReviewService;
        private readonly IMapper _mapper;
        private readonly IUserRatingUpdateJob _userRatingUpdateJob;
        private readonly IVehicleRatingUpdateJob _vehicleRatingUpdateJob;
        private readonly INotificationJob _notificationJob;

        public ReviewService(IRentalService rentalService, IUnitOfWork unitOfWork, ILogger<ReviewService> logger, IRentalReviewService rentalReviewService, IMapper mapper, IUserRatingUpdateJob ratingUpdateJob, IVehicleRatingUpdateJob vehicleRatingUpdateJob, INotificationJob notificationJob)
        {
            _rentalService = rentalService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _rentalReviewService = rentalReviewService;
            _mapper = mapper;
            _userRatingUpdateJob = ratingUpdateJob;
            _vehicleRatingUpdateJob = vehicleRatingUpdateJob;
            _notificationJob = notificationJob;
        }

        private string? CheckReviewRates(CreateReviewDto reviewDto, string role)
        {
            var errors = new List<string>();
            if ((reviewDto.Behavior < 1 || reviewDto.Behavior > 5))
                errors.Add("Behavior rate must be between 1 and 5");
            if (reviewDto.Punctuality < 1 || reviewDto.Punctuality > 5)
                errors.Add("Punctuality rate must be between 1 and 5");
            if (role == Roles.Renter)
            {
                if (reviewDto.Care == null || reviewDto.Care < 1 || reviewDto.Care > 5)
                    errors.Add("Care rate must be between 1 and 5");
                if (reviewDto.Truthfulness != null)
                    errors.Add("Truthfulness rate must be null for renter review");
            }
            else
            {
                if (reviewDto.Truthfulness == null || reviewDto.Truthfulness < 1 || reviewDto.Truthfulness > 5)
                    errors.Add("Truthfulness rate must be between 1 and 5");
                if (reviewDto.Care != null)
                    errors.Add("Care rate must be null for customer review");
            }
            return errors.Count > 0 ? string.Join(", ", errors) : null;
        }
        private async Task<ResponseDto<T>> CreateReviewAsync<T>((int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId) rental, CreateReviewDto reviewDto, string role, int userId) where T : Review
        {
            _logger.LogInformation("Creating {Role} review for rental {RentalId} by user {UserId}", role, reviewDto.RentalId, userId);

            if (role == Roles.Customer)
            {
                if (rental.CustomerReviewId != 0)
                {
                    _logger.LogWarning("User {UserId} attempted to create duplicate customer review for rental {RentalId}", userId, reviewDto.RentalId);
                    return new ResponseDto<T>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "You have already reviewed this rental"
                    };
                }


                if (rental.CustomerId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to create review for rental {RentalId} they don't own", userId, reviewDto.RentalId);
                    return new ResponseDto<T>
                    {
                        StatusCode = StatusCodes.Forbidden,
                        Message = "You are not allowed to review this rental"
                    };
                }

                _logger.LogDebug("Creating CustomerReview for rental {RentalId}", reviewDto.RentalId);
                return new ResponseDto<T>
                {
                    StatusCode = StatusCodes.OK,
                    Data = new CustomerReview
                    {
                        Behavior = reviewDto.Behavior,
                        Punctuality = reviewDto.Punctuality,
                        Comments = reviewDto.Comments,
                        Truthfulness = reviewDto.Truthfulness.Value,
                        RentalId = reviewDto.RentalId,
                        CustomerId = userId,
                    } as T
                };
            }

            // renter
            if (rental.RenterReviewId != 0)
            {
                _logger.LogWarning("User {UserId} attempted to create duplicate renter review for rental {RentalId}", userId, reviewDto.RentalId);
                return new ResponseDto<T>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "You have already reviewed this rental"
                };
            }

            if (rental.RenterId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to create review for rental {RentalId} they didn't rent", userId, reviewDto.RentalId);
                return new ResponseDto<T>
                {
                    StatusCode = StatusCodes.Forbidden,
                    Message = "You are not allowed to review this rental"
                };
            }

            _userRatingUpdateJob.ScheduleUserRatingUpdate(rental.CustomerId);

            _logger.LogDebug("Creating RenterReview for rental {RentalId}", reviewDto.RentalId);
            return new ResponseDto<T>
            {
                StatusCode = StatusCodes.OK,
                Data = new RenterReview
                {
                    Behavior = reviewDto.Behavior,
                    Punctuality = reviewDto.Punctuality,
                    Comments = reviewDto.Comments,
                    Care = reviewDto.Care.Value,
                    RentalId = reviewDto.RentalId,
                    RenterId = userId,
                } as T
            };
        }
        public async Task<ResponseDto<GetReviewDto>> CreateReviewAsync(CreateReviewDto reviewDto, int userId, string role)
        {
            _logger.LogInformation("Received request to create review for rental {RentalId} by user {UserId} with role {Role}",
                reviewDto.RentalId, userId, role);

            if (CheckReviewRates(reviewDto, role) is string ratesErrors)
            {
                _logger.LogWarning("Review validation failed for rental {RentalId} by user {UserId}: {ErrorMessage}",
                    reviewDto.RentalId, userId, ratesErrors);
                return new ResponseDto<GetReviewDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = ratesErrors
                };
            }

            _logger.LogDebug("Fetching rental data for rental {RentalId}", reviewDto.RentalId);
            var rentalResponse = await _rentalService.GetReviewRentalValidationProperties(reviewDto.RentalId);
            
            if(rentalResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to retrieve rental {RentalId}: {ErrorMessage}",
                    reviewDto.RentalId, rentalResponse.Message);
                return new ResponseDto<GetReviewDto>
                {
                    StatusCode = rentalResponse.StatusCode,
                    Message = rentalResponse.Message
                };
            }

            var rental = rentalResponse.Data;
            
            _logger.LogDebug("Rental data retrieved successfully for rental {RentalId}", reviewDto.RentalId);

            var reviewResponse = await CreateReviewAsync<Review>(rental.Value, reviewDto, role, userId);

            if (reviewResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to create review object for rental {RentalId} by user {UserId}: {ErrorMessage}",
                    reviewDto.RentalId, userId, reviewResponse.Message);
                return new ResponseDto<GetReviewDto>
                {
                    StatusCode = reviewResponse.StatusCode,
                    Message = reviewResponse.Message
                };
            }

            var review = reviewResponse.Data;
            _logger.LogDebug("Adding review to database for rental {RentalId}", reviewDto.RentalId);

            var reviewRentalCreationResponse = await _rentalReviewService.CreateReviewUpdateRentalAsync(review, role, review.RentalId);

            if (reviewRentalCreationResponse.StatusCode != StatusCodes.OK)
            {
                _logger.LogWarning("Failed to add review to database for rental {RentalId} by user {UserId}: {ErrorMessage}",
                    reviewDto.RentalId, userId, reviewRentalCreationResponse.Message);
                return new ResponseDto<GetReviewDto>
                {
                    StatusCode = reviewRentalCreationResponse.StatusCode,
                    Message = reviewRentalCreationResponse.Message
                };
            }

            _userRatingUpdateJob.ScheduleUserRatingUpdate(rental.Value.RenterId);
            _vehicleRatingUpdateJob.ScheduleVehicleRatingUpdate(rental.Value.VehicleId);
            //notification
            if(role == Roles.Customer)
            {
                var dto = new CreateNotificationDto
                {
                    Content = NotificationContent.Review,
                    ReceiverId = rental.Value.RenterId,
                    TargetId = review.Id,
                    TargetType = DATA.Models.Enums.TargetType.CustomerReview,
                };
                await _notificationJob.SendNotificationAsync(dto);
            }
            else
            {
                var dto = new CreateNotificationDto
                {
                    Content = NotificationContent.Review,
                    ReceiverId = rental.Value.CustomerId,
                    TargetId = review.Id,
                    TargetType = DATA.Models.Enums.TargetType.RenterReview,
                };
                await _notificationJob.SendNotificationAsync(dto);
            }


            _logger.LogInformation("Successfully created review for rental {RentalId} by user {UserId}",
                reviewDto.RentalId, userId);
            return new ResponseDto<GetReviewDto>
            {
                StatusCode = StatusCodes.Created,
                Data = new GetReviewDto
                {
                    Id = review.Id,
                    CreatedAt = review.CreatedAt,
                    Behavior = review.Behavior,
                    Punctuality = review.Punctuality,
                    Comments = review.Comments,
                    RentalId = review.RentalId,
                    Care = reviewDto.Care,
                    Truthfulness = reviewDto.Truthfulness,
                }
            };
        }

        public async Task<ResponseDto<GetReviewDto>> GetReviewAsync(int reviewId)
        {
            _logger.LogInformation("Received request to get review with ID {ReviewId}", reviewId);

            GetReviewDto result = null;

            result = _mapper.Map<GetReviewDto>(await _unitOfWork.CustomerReviews.GetAsync(reviewId));

            if(result == null)
            {
                _logger.LogInformation("No customer review found with ID {ReviewId}", reviewId);
                result = _mapper.Map<GetReviewDto>(await _unitOfWork.RenterReviews.GetAsync(reviewId));
            }

            if(result == null)
            {
                _logger.LogWarning("No review found with ID {ReviewId}", reviewId);
                return new ResponseDto<GetReviewDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "No review found with this ID"
                };
            }

            _logger.LogInformation("Successfully retrieved review with ID {ReviewId}", reviewId);
            return new ResponseDto<GetReviewDto>
            {
                StatusCode = StatusCodes.OK,
                Data = result
            };
        }
    }
}
