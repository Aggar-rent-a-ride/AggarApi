using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CORE.DTOs.Report;
using CORE.Services;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models.Enums;
using DATA.Models;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using Azure.Core;
using CORE.Constants;
using DATA.DataAccess.Repositories.IRepositories;

namespace UnitTests.CORE.Services
{
    [TestFixture]
    public class ReportServiceTests
    {
        private Mock<IMapper> _mockMapper;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBaseRepository<Report>> _mockReportRepository;
        private ReportService _reportService;

        [SetUp]
        public void SetUp()
        {
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _reportService = new ReportService(_mockMapper.Object, _mockUnitOfWork.Object);
            _mockReportRepository = new Mock<IBaseRepository<Report>>();
            _mockUnitOfWork.Setup(u => u.Reports).Returns(_mockReportRepository.Object);
        }
        [Test]
        public async Task CreateReportAsync_ValidInput_SuccessfulCreation()
        {
            // Arrange
            var reportDto = new CreateReportDto
            {
                TargetId = 123,
                TargetType = TargetType.AppUser,
                Description = "Harassment"
            };

            var reporterId = 456;

            var mockReport = new Report
            {
                Id = 789,
                ReporterId = reporterId,
                TargetAppUserId = reportDto.TargetId,
                Description = reportDto.Description
            };

            _mockMapper.Setup(m => m.Map<Report>(It.IsAny<CreateReportDto>())).Returns(mockReport);
            _mockUnitOfWork.Setup(u => u.AppUsers.CheckAnyAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), null)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _reportService.CreateReportAsync(reportDto, reporterId);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Created));
        }
        [Test]
        public async Task CreateReportAsync_MissingTargetId_BadRequest()
        {
            // Arrange
            var reportDto = new CreateReportDto
            {
                TargetId = null,
                TargetType = TargetType.AppUser,
                Description = "Harassment"
            };

            var reporterId = 456;

            // Act
            var result = await _reportService.CreateReportAsync(reportDto, reporterId);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
        }
        [Test]
        public async Task CreateReportAsync_NonExistentTarget_BadRequest()
        {
            // Arrange
            var reportDto = new CreateReportDto
            {
                TargetId = 123,
                TargetType = TargetType.AppUser,
                Description = "Harassment"
            };

            var reporterId = 456;

            _mockUnitOfWork.Setup(u => u.AppUsers.CheckAnyAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), null)).ReturnsAsync(false);

            // Act
            var result = await _reportService.CreateReportAsync(reportDto, reporterId);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
        }
        [Test]
        public async Task CreateReportAsync_DatabaseCommitFailure_InternalServerError()
        {
            // Arrange
            var reportDto = new CreateReportDto
            {
                TargetId = 123,
                TargetType = TargetType.AppUser,
                Description = "Harassment"
            };

            var reporterId = 456;

            var mockReport = new Report
            {
                Id = 789,
                ReporterId = reporterId,
                TargetAppUserId = reportDto.TargetId,
                Description = reportDto.Description
            };

            _mockMapper.Setup(m => m.Map<Report>(It.IsAny<CreateReportDto>())).Returns(mockReport);
            _mockUnitOfWork.Setup(u => u.AppUsers.CheckAnyAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), null)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(0);

            // Act
            var result = await _reportService.CreateReportAsync(reportDto, reporterId);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.InternalServerError));
            Assert.That(result.Message, Is.EqualTo("Failed to create report"));
        }
        [Test]
        public async Task CreateReportAsync_InvalidTargetType_BadRequest()
        {
            // Arrange
            var reportDto = new CreateReportDto
            {
                TargetId = 123,
                TargetType = TargetType.None,
                Description = "Harassment"
            };

            var reporterId = 456;

            // Act
            var result = await _reportService.CreateReportAsync(reportDto, reporterId);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
            Assert.That(result.Message, Is.EqualTo("Target type is required"));
        }
    }
}
