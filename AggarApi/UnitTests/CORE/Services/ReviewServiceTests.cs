using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Review;
using CORE.DTOs.Vehicle;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Mock<IRentalReviewService> _mockRentalReviewService;
        private ReviewService _reviewService;

        [SetUp]
        public void SetUp()
        {
            _mockRentalService = new Mock<IRentalService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ReviewService>>();
            _mockRentalReviewService = new Mock<IRentalReviewService>();
            _reviewService = new ReviewService(
                _mockRentalService.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockRentalReviewService.Object
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
                .ReturnsAsync(new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?> { StatusCode = StatusCodes.BadRequest });

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
                new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?> 
                { 
                    StatusCode = StatusCodes.OK,
                    Data = (1, 1, 1, 1, 1)
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
                new ResponseDto<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?>
                {
                    StatusCode = StatusCodes.OK,
                    Data = (1, 1, 0, 1, 1)
                });

            // Act
            var result = await _reviewService.CreateReviewAsync(createReviewDto, 2, Roles.Renter);

            // Assert
            Assert.That("You are not allowed to review this rental", Is.EqualTo(result.Message));
        }
    }
}
