using AutoMapper;
using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Payment;
using CORE.DTOs.Rental;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.Constants.Includes;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
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
        private readonly IQrCodeService _qrCodeService;
        private readonly IHashingService _hashingService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationJob _notificationJob;
        private readonly PaymentPolicy _paymentPolicy;

        public RentalService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RentalService> logger,
            IQrCodeService qrCodeService, IHashingService hashingService, IEmailService emailService,
            IEmailTemplateRendererService emailTemplateRendererService,
            IPaymentService paymentService, INotificationJob notificationJob, IOptions<PaymentPolicy> paymentPolicy)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _qrCodeService = qrCodeService;
            _hashingService = hashingService;
            _emailService = emailService;
            _emailTemplateRendererService = emailTemplateRendererService;
            _paymentService = paymentService;
            _notificationJob = notificationJob;
            _paymentPolicy = paymentPolicy.Value;
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
        public async Task<ResponseDto<IEnumerable<GetRentalsByUserIdDto>>> GetRentalsByUserIdAsync(int userId)
        {
            _logger.LogInformation("Getting rentals for user with ID: {UserId}", userId);

            var rentals = await _unitOfWork.Rentals.GetRentalsByUserIdAsync(userId);

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

        public async Task<ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?>> GetReviewRentalValidationProperties(int rentalId)
        {
            _logger.LogInformation("Getting rental with ID: {RentalId}", rentalId);

            var rental = await _unitOfWork.Rentals.GetRentalByIdIncludingBookingThenIncludingVehicleAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning("Rental with ID: {RentalId} not found", rentalId);
                return new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Rental not found."
                };
            }
            
            _logger.LogInformation("Successfully retrieved rental with ID: {RentalId}", rentalId);
            return new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?>
            {
                StatusCode = StatusCodes.OK,
                Data = rental
            };
        }

        /*private string GetRentalStatus(Booking booking)
        {
            if (booking.Status == DATA.Models.Enums.BookingStatus.Canceled)
                return RentalStatus.Cancelled;

            // accepted
            if (booking.StartDate > DateTime.UtcNow)
                return RentalStatus.NotStarted;

            if (booking.StartDate <= DateTime.UtcNow && booking.EndDate >= DateTime.UtcNow)
                return RentalStatus.InProgress;

            return RentalStatus.Completed;
        }*/

        public async Task<ResponseDto<PagedResultDto<IEnumerable<RentalHistoryItemDto>>>> GetUserRentalHistoryAsync(int userId, int pageNo, int pageSize, int maxPageSize = 50)
        {
            _logger.LogInformation("Getting rental history for user with ID: {UserId}", userId);
            if (PaginationHelpers.ValidatePaging(pageNo, pageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {PaginationError}", paginationError);
                return new ResponseDto<PagedResultDto<IEnumerable<RentalHistoryItemDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }
            
            var rentals = await _unitOfWork.Rentals.GetUserRentalHistoryAsync(userId, pageNo, pageSize);

            _logger.LogInformation("Successfully retrieved rental history for user with ID: {UserId}", userId);

            var result = new List<RentalHistoryItemDto>();
            var count = await _unitOfWork.Rentals.GetUserRentalHistoryCountAsync(userId);
            foreach (var rental in rentals)
            {
                var rentalHistoryItem = new RentalHistoryItemDto
                {
                    Id = rental.Id,
                    StartDate = rental.Booking.StartDate,
                    EndDate = rental.Booking.EndDate,
                    TotalDays = rental.Booking.TotalDays,
                    FinalPrice = rental.Booking.FinalPrice,
                    RentalStatus = rental.Status,
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

            return new ResponseDto<PagedResultDto<IEnumerable<RentalHistoryItemDto>>>
            {
                StatusCode = StatusCodes.OK,
                Data = PaginationHelpers.CreatePagedResult(result.AsEnumerable(), pageNo, pageSize, count),
            };
        }

        public async Task<CreatedRentalDto> CreateRentalAsync(Booking booking)
        {
            Guid guid = Guid.NewGuid();
            string qrData = $"{booking.Id}, {guid.ToString()}";

            string qrToken = _qrCodeService.GenerateQrHashToken(qrData);
            byte[] qrCodeImage = _qrCodeService.GenerateQrCode(qrToken);
            string qrCodeBase64 = Convert.ToBase64String(qrCodeImage);
            string hashedQrToken = _hashingService.Hash(qrToken);
            Console.WriteLine($"qr token: {qrCodeBase64}");


            Rental rental = new Rental
            {
                BookingId = booking.Id,
                hashedQrToken = hashedQrToken
            };

            await _unitOfWork.Rentals.AddOrUpdateAsync(rental);

            int changes = await _unitOfWork.CommitAsync();

            if (changes == 0)
            {
                _logger.LogError($"Failed to create rental for booking {booking.Id}");
                return new CreatedRentalDto { RentalId = 0 };
            }

            await _emailService.SendEmailAsync(booking.Vehicle.Renter.Email, EmailSubject.RentalConfirmationQRCode, 
                await _emailTemplateRendererService.RenderTemplateAsync(Templates.RentalConfirmationQRCode, 
                new {
                    BookingId = System.Web.HttpUtility.HtmlEncode(booking.Id.ToString()),
                    VehicleBrand = System.Web.HttpUtility.HtmlEncode(booking.Vehicle.VehicleBrand.Name),
                    VehicleModel = System.Web.HttpUtility.HtmlEncode(booking.Vehicle.Model),
                    StartDate = System.Web.HttpUtility.HtmlEncode(booking.StartDate.ToString("MMMM dd, yyyy hh: mm tt")), 
                    EndDate = System.Web.HttpUtility.HtmlEncode(booking.EndDate.ToString("MMMM dd, yyyy hh: mm tt")) }));

            // notify renter
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = "Your rental QR code confirmation has been sent to your email.",
                ReceiverId = booking.Vehicle.RenterId,
                TargetId = rental.Id,
                TargetType = DATA.Models.Enums.TargetType.Rental
            });

            return new CreatedRentalDto
            {
                QrCodeBase64 = qrCodeBase64,
                RentalId = rental.Id,
            };
        }

        public async Task<ResponseDto<object>> ConfirmRentalAsync(int customerId, int rentalId, string receivedQrCodeToken)
        {
            Rental? rental = await _unitOfWork.Rentals.FindAsync(r => r.Id == rentalId, includes: [RentalIncludes.Booking, $"{RentalIncludes.Booking}.{BookingIncludes.Vehicle}"]);

            if(rental == null || customerId != rental.Booking.CustomerId)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.NotFound,
                    Message = "Rental is not found"
                };
            }

            if(rental.Booking.StartDate ==  DateTime.UtcNow /* || staus not pending*/)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Rental can not confirmed in this time"
                };
            }

            string recievedHashedQrCodeToken = _hashingService.Hash(receivedQrCodeToken);
            if (recievedHashedQrCodeToken != rental.hashedQrToken)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "QR Code is not valid"
                };
            }

            bool transferResult = await TransferToRenter(rental);

            if (!transferResult)
            {
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = "QR Code Validated Successfuly, but Transfer Not Succeded"
                };
            }

            // notify renter
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = "Your Rental Confirmed Successfuly, Your payment will be transfered to your bank account in 3 days.",
                ReceiverId = rental.Booking.Vehicle.RenterId,
                TargetId = rental.Id,
                TargetType = DATA.Models.Enums.TargetType.Rental
            });

            // notify customer
            await _notificationJob.SendNotificationAsync(new DTOs.Notification.CreateNotificationDto
            {
                Content = "You have Successfuly confirm your rental.",
                ReceiverId = rental.Booking.CustomerId,
                TargetId = rental.Id,
                TargetType = DATA.Models.Enums.TargetType.Rental
            });


            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "Rental Cofirmed Successfuly"
            };
        }

        private async Task<bool> TransferToRenter(Rental rental)
        {
            long platformFee = (long)(rental.Booking.FinalPrice * _paymentPolicy.FeesPercentage * 100);
            long renterAmount = (long)(rental.Booking.FinalPrice * 100) - platformFee;
            Transfer? transfer =  await _paymentService.TransferToRenterAsync(rental.Booking.PaymentIntentId, rental.Booking.Vehicle.Renter.StripeAccount.StripeAccountId, rental.Id, renterAmount);

            if (transfer == null)
            {
                return false;
            }

            rental.PaymentTransferId = transfer.Id;
            await _unitOfWork.Rentals.AddOrUpdateAsync(rental);

            int changes = await _unitOfWork.CommitAsync();

            return changes > 0;
        }

        public async Task HandleTransferAsync(int rentalId)
        {
            Rental? rental = await _unitOfWork.Rentals.GetAsync(rentalId);
            if (rental == null)
            {
                _logger.LogError($"Rental {rentalId} not found");
                return;
            }
            rental.Status = DATA.Models.Enums.RentalStatus.Confirmed; 
            
            await _unitOfWork.Rentals.AddOrUpdateAsync(rental);

            await _unitOfWork.CommitAsync();
        }
    }
}
