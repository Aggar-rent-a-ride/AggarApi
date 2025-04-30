using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Paths;
using CORE.DTOs.Vehicle;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Options;
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
    public class VehicleServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IFileService> _mockFileService;
        private Mock<IOptions<Paths>> _mockPaths;
        private Mock<IMapper> _mockMapper;
        private Mock<IGeoapifyService> _mockGeoapifyService;
        private Paths _paths;
        private Mock<IVehicleRepository> _mockVehicleRepository;
        private VehicleService _vehicleService;
        private Mock<IReviewService> _mockReviewService;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockFileService = new Mock<IFileService>();
            _mockPaths = new Mock<IOptions<Paths>>();
            _mockMapper = new Mock<IMapper>();
            _mockGeoapifyService = new Mock<IGeoapifyService>();
            _mockVehicleRepository = new Mock<IVehicleRepository>();
            _mockReviewService = new Mock<IReviewService>();

            _paths = new Paths { VehicleImages = "vehicle_images" };
            _mockPaths.Setup(p => p.Value).Returns(_paths);
            _vehicleService = new VehicleService(
                 _mockUnitOfWork.Object,
                 _mockFileService.Object,
                 _mockPaths.Object,
                 _mockMapper.Object,
                 _mockGeoapifyService.Object,
                _mockReviewService.Object
             );
            _mockUnitOfWork.Setup(u=>u.Vehicles).Returns(_mockVehicleRepository.Object);
        }
        [Test]
        public async Task CreateVehicleAsync_ShouldReturnBadRequest_WhenRenterIdIsNull()
        {
            // Arrange
            var createVehicleDto = new CreateVehicleDto();

            // Act
            var result = await _vehicleService.CreateVehicleAsync(createVehicleDto, null);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateVehicleAsync_ShouldReturnBadRequest_WhenRenterIdIsZero()
        {
            // Arrange
            var createVehicleDto = new CreateVehicleDto();

            // Act
            var result = await _vehicleService.CreateVehicleAsync(createVehicleDto, 0);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateVehicleAsync_ShouldReturnBadRequest_WhenDtoValidationFails()
        {
            // Arrange
            var createVehicleDto = new CreateVehicleDto
            {
                NumOfPassengers = 0,
            };

            // Act
            var result = await _vehicleService.CreateVehicleAsync(createVehicleDto, 1);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateVehicleAsync_ShouldReturnInternalServerError_WhenMappingFails()
        {
            // Arrange
            var createVehicleDto = new CreateVehicleDto
            {
                NumOfPassengers = 4,
                Year = 2020,
                MainImage = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object,
                PricePerDay = 100,
                Location = new Location(),
                VehicleTypeId = 1,
                VehicleBrandId = 1
            };
            _mockMapper.Setup(m => m.Map<Vehicle>(createVehicleDto)).Returns((Vehicle)null);

            // Act
            var result = await _vehicleService.CreateVehicleAsync(createVehicleDto, 1);

            // Assert
            Assert.That(StatusCodes.InternalServerError, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateVehicleAsync_ShouldReturnBadRequest_WhenFileUploadFails()
        {
            // Arrange
            var createVehicleDto = new CreateVehicleDto
            {
                NumOfPassengers = 4,
                Year = 2020,
                MainImage = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object,
                PricePerDay = 100,
                Location = new Location(),
            };
            _mockMapper.Setup(m => m.Map<Vehicle>(createVehicleDto)).Returns(new Vehicle());
            _mockFileService
                .Setup(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<List<string>>()))
                .ReturnsAsync((string)null); 

            // Act
            var result = await _vehicleService.CreateVehicleAsync(createVehicleDto, 1);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task CreateVehicleAsync_ShouldReturnInternalServerError_WhenDatabaseSaveFails()
        {
            // Arrange
            var createVehicleDto = new CreateVehicleDto
            {
                NumOfPassengers = 4,
                Year = 2020,
                MainImage = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object,
                PricePerDay = 100,
                Location = new Location(),
                VehicleBrandId = 1,
                VehicleTypeId = 1
            };
            _mockMapper.Setup(m => m.Map<Vehicle>(createVehicleDto)).Returns(new Vehicle());
            _mockFileService
                .Setup(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<List<string>>()))
                .ReturnsAsync("valid/path.png");
            _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(0); // Simulate database save failure

            // Act
            var result = await _vehicleService.CreateVehicleAsync(createVehicleDto, 1);

            // Assert
            Assert.That(StatusCodes.InternalServerError, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task UpdateVehicleAsync_ShouldReturnBadRequest_WhenRenterIdIsNull()
        {
            // Arrange
            var updateVehicleDto = new UpdateVehicleDto { Id = 1 };

            // Act
            var result = await _vehicleService.UpdateVehicleAsync(updateVehicleDto, null);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task UpdateVehicleAsync_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var updateVehicleDto = new UpdateVehicleDto { Id = 0 };

            // Act
            var result = await _vehicleService.UpdateVehicleAsync(updateVehicleDto, 1);

            // Assert
            Assert.That(StatusCodes.BadRequest, Is.EqualTo(result.StatusCode));
        }
        [Test]
        public async Task UpdateVehicleAsync_ShouldReturnBadRequest_WhenVehicleDoesNotExist()
        {
            // Arrange
            var updateVehicleDto = new UpdateVehicleDto { Id = 1, NumOfPassengers = 1, Year = 1995, PricePerDay = 10, Location = new Location(),
                VehicleTypeId = 1,
                VehicleBrandId = 1
            };
            _mockUnitOfWork.Setup(u => u.Vehicles.GetAsync(updateVehicleDto.Id)).ReturnsAsync((Vehicle)null);

            // Act
            var result = await _vehicleService.UpdateVehicleAsync(updateVehicleDto, 1);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
        }
    }
}
