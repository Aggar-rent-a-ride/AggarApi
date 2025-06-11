using AutoMapper;
using CORE.BackgroundJobs;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Rental;
using CORE.DTOs.Review;
using CORE.DTOs.Vehicle;
using CORE.Services;
using CORE.Services.IServices;
using DATA.Constants.Enums;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CORE.Services
{
    [TestFixture]
    public class ReviewServiceTests
    {
        private Mock<IRentalService> _mockRentalService;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ILogger<ReviewService>> _mockLogger;
        private Mock<ILogger<UserReviewService>> _mockLogger2;
        private Mock<IRentalReviewService> _mockRentalReviewService;
        private ReviewService _reviewService;
        private UserReviewService _userReviewService;
        private Mock<IMapper> _mockMapper;

        [SetUp]
        public void SetUp()
        {
            _mockRentalService = new Mock<IRentalService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ReviewService>>();
            _mockLogger2 = new Mock<ILogger<UserReviewService>>();
            _mockRentalReviewService = new Mock<IRentalReviewService>();
            _mockMapper = new Mock<IMapper>();
            _reviewService = new ReviewService(
                _mockRentalService.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockRentalReviewService.Object,
                _mockMapper.Object,
                null, 
                null,
                null
            );
            _userReviewService = new UserReviewService(
                _mockUnitOfWork.Object,
                _mockLogger2.Object,
                _mockMapper.Object,
                _mockRentalService.Object
            );
        }
        [Test]
        public async Task CreateReviewAsync_ShouldReturnBadRequest_WhenReviewerRoleIsRenterAndCareIsNull()
        {
            // Arrange
            var createReviewDto = new CreateReviewDto { Behavior = 4, Punctuality = 3, Care = null };

            // Act
            var result = await _reviewService.CreateReviewAsync(createReviewDto, 1, Roles.Renter);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateReviewAsync_ShouldReturnBadRequest_WhenReviewerRoleIsRenterAndTruthfulnessExist()
        {
            // Arrange
            var createReviewDto = new CreateReviewDto {Behavior = 4, Punctuality = 3,  Care = 1.5, Truthfulness = 5 };

            // Act
            var result = await _reviewService.CreateReviewAsync(createReviewDto, 1, Roles.Renter);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateReviewAsync_ShouldFail_WhenGetReviewRentalValidationPropertiesFails()
        {
            // Arrange
            var createReviewDto = new CreateReviewDto { Behavior = 4, Punctuality = 3, Care = 1.5 };
            _mockRentalService.Setup(r => r.GetReviewRentalValidationProperties(It.IsAny<int>()))
                .ReturnsAsync(new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?> { StatusCode = StatusCodes.BadRequest });

            // Act
            var result = await _reviewService.CreateReviewAsync(createReviewDto, 1, Roles.Renter);

            // Assert
            Assert.That(StatusCodes.OK, Is.Not.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateReviewAsync_ShouldFail_WhenReviewerReviewedRentalBefore()
        {
            // Arrange
            var createReviewDto = new CreateReviewDto {RentalId = 1, Behavior = 4, Punctuality = 3, Care = 1.5 };
            _mockRentalService.Setup(r => r.GetReviewRentalValidationProperties(It.IsAny<int>()))
                .ReturnsAsync(
                new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?> 
                { 
                    StatusCode = StatusCodes.OK,
                    Data = (1, 1, 1, 1, 1, 1)
                });

            // Act
            var result = await _reviewService.CreateReviewAsync(createReviewDto, 1, Roles.Renter);

            // Assert
            Assert.That("You have already reviewed this rental", Is.EqualTo(result.Message));
        }
        [Test]
        public async Task CreateReviewAsync_ShouldFail_WhenRenterIdNotEqualToUserId()
        {
            // Arrange
            var createReviewDto = new CreateReviewDto { RentalId = 1, Behavior = 4, Punctuality = 3, Care = 1.5 };
            _mockRentalService.Setup(r => r.GetReviewRentalValidationProperties(It.IsAny<int>()))
                .ReturnsAsync(
                new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?>
                {
                    StatusCode = StatusCodes.OK,
                    Data = (1, 1, 0, 1, 1, 1)
                });

            // Act
            var result = await _reviewService.CreateReviewAsync(createReviewDto, 2, Roles.Renter);

            // Assert
            Assert.That("You are not allowed to review this rental", Is.EqualTo(result.Message));
        }
        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        public async Task GetUserReviewsAsync_ShouldFail_WhenInvalidPagingParameters(int pageNo, int pageSize)
        {
            // Arrange
            var userId = 1;
            // Act
            var result = await _userReviewService.GetUserReviewsAsync(userId, pageNo, pageSize);
            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task GetUserReviewsAsync_ShouldReturnBadRequest_WhenGetRentalsByUserIdFails()
        {
            // Arrange
            var userId = 1;
            var pageNo = 1;
            var pageSize = 10;
            _mockRentalService.Setup(r => r.GetRentalsByUserIdAsync(userId, pageNo, pageSize, 100))
                .ReturnsAsync(new ResponseDto<IEnumerable<GetRentalsByUserIdDto>> { StatusCode = StatusCodes.BadRequest });
            // Act
            var result = await _userReviewService.GetUserReviewsAsync(userId, pageNo, pageSize);
            // Assert
            Assert.That(StatusCodes.OK, Is.Not.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task GetUserReviewsAsync_ShouldReturnBadRequest_WhenEmptyUserRentalsResponseData()
        {
            // Arrange
            var userId = 1;
            var pageNo = 1;
            var pageSize = 10;
            _mockRentalService.Setup(r => r.GetRentalsByUserIdAsync(userId, pageNo, pageSize, 100))
                .ReturnsAsync(new ResponseDto<IEnumerable<GetRentalsByUserIdDto>> { StatusCode = StatusCodes.OK, Data = new List<GetRentalsByUserIdDto>() });
            // Act
            var result = await _userReviewService.GetUserReviewsAsync(userId, pageNo, pageSize);
            // Assert
            Assert.That("No rentals found for this user", Is.EqualTo(result.Message));
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task GetUserReviewsAsync_ShouldReturnSuccess_WhenEmptyResult()
        {
            // Arrange
            var userId = 1;
            var pageNo = 1;
            var pageSize = 10;
            var rentals = new List<GetRentalsByUserIdDto>
            {
                new GetRentalsByUserIdDto { Id = 1, BookingId = 1, CustomerReviewId = 0, RenterReviewId = 0, Booking = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking { Id = 1, CustomerId = 1, Vehicle = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking.GetRentalsByUserIdDtoVehicle { Id = 1, RenterId = 1 } } },
                new GetRentalsByUserIdDto { Id = 2, BookingId = 2, CustomerReviewId = 0, RenterReviewId = 0, Booking = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking { Id = 2, CustomerId = 1, Vehicle = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking.GetRentalsByUserIdDtoVehicle { Id = 2, RenterId = 2 } } }
            };
            _mockRentalService.Setup(r => r.GetRentalsByUserIdAsync(userId, pageNo, pageSize, 100))
                .ReturnsAsync(new ResponseDto<IEnumerable<GetRentalsByUserIdDto>> { StatusCode = StatusCodes.OK, Data = rentals });
            _mockRentalService.Setup(r => r.GetRentalByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ResponseDto<GetRentalDto?> { StatusCode = StatusCodes.BadRequest });

            _mockUnitOfWork.Setup(u => u.RenterReviews.FindAsync(
                    It.IsAny<Expression<Func<RenterReview, bool>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<Expression<Func<RenterReview, object>>>(),
                    It.IsAny<OrderBy>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<RenterReview>());

            _mockUnitOfWork.Setup(u => u.Rentals.GetRentalsByUserIdCountAsync( It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(1);

            // Act
            var result = await _userReviewService.GetUserReviewsAsync(userId, pageNo, pageSize);
            // Assert
            Assert.That(result.Data.Data, Is.Empty);
        }
        [Test]
        public async Task GetUserReviewsAsync_ShouldSucceed()
        {
            // Arrange
            var userId = 1;
            var pageNo = 1;
            var pageSize = 10;
            var rentals = new List<GetRentalsByUserIdDto>
            {
                new GetRentalsByUserIdDto { Id = 1, BookingId = 1, CustomerReviewId = 0, RenterReviewId = 0, Booking = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking { Id = 1, CustomerId = 1, Vehicle = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking.GetRentalsByUserIdDtoVehicle { Id = 1, RenterId = 1 } } },
                new GetRentalsByUserIdDto { Id = 2, BookingId = 2, CustomerReviewId = 0, RenterReviewId = 0, Booking = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking { Id = 2, CustomerId = 1, Vehicle = new GetRentalsByUserIdDto.GetRentalsByUserIdDtoBooking.GetRentalsByUserIdDtoVehicle { Id = 2, RenterId = 2 } } }
            };
            _mockRentalService.Setup(r => r.GetRentalsByUserIdAsync(userId, pageNo, pageSize, 100))
                .ReturnsAsync(new ResponseDto<IEnumerable<GetRentalsByUserIdDto>> { StatusCode = StatusCodes.OK, Data = rentals });
            _mockRentalService.Setup(r => r.GetRentalByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ResponseDto<GetRentalDto?> { StatusCode = StatusCodes.BadRequest });

            _mockUnitOfWork.Setup(u => u.RenterReviews.FindAsync(
                    It.IsAny<Expression<Func<RenterReview, bool>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<Expression<Func<RenterReview, object>>>(),
                    It.IsAny<OrderBy>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<RenterReview>());

            _mockUnitOfWork.Setup(u => u.Rentals.GetRentalsByUserIdCountAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            _mockMapper.Setup(r => r.Map<IEnumerable<SummarizedReviewDto>>(It.IsAny<IEnumerable<RenterReview>>()))
                .Returns(new List<SummarizedReviewDto>
                {
                    new SummarizedReviewDto(),
                    new SummarizedReviewDto()
                });
            // Act
            var result = await _userReviewService.GetUserReviewsAsync(userId, pageNo, pageSize);
            // Assert
            Assert.That(result.Message, Is.EqualTo(null));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.OK));
        }
    }
}
