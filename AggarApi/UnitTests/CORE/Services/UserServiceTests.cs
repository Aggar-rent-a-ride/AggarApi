using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using CORE.Constants;
using CORE.DTOs.AppUser;
using CORE.DTOs.Auth;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace UnitTests.CORE.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<UserService>> _mockLogger;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IEmailTemplateRendererService> _mockEmailTemplateRendererService;
        private Mock<IOptions<WarningManagement>> _mockWarningManagement; 
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockEmailService = new Mock<IEmailService>();
            _mockEmailTemplateRendererService = new Mock<IEmailTemplateRendererService>();
            _mockWarningManagement = new Mock<IOptions<WarningManagement>>();
            _userService = new UserService(_mockUnitOfWork.Object, _mockLogger.Object, _mockMapper.Object, null, _mockEmailService.Object, _mockEmailTemplateRendererService.Object, _mockWarningManagement.Object, null, null );
        }
        [Test]
        public async Task FindUsersAsync_InvalidPagination_ReturnsBadRequest()
        {
            // Arrange
            var pageNo = 0; // Invalid page number
            var pageSize = 200; // Invalid page size
            var maxPageSize = 100;

            // Act
            var result = await _userService.FindUsersAsync(null, pageNo, pageSize, maxPageSize);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
            Assert.That(result.Message, Is.Not.Null);
        }
        [Test]
        public async Task FindUsersAsync_ValidPaginationNoSearchKey_ReturnsAllUsers()
        {
            // Arrange
            var pageNo = 1;
            var pageSize = 10;
            var usersFromDb = new List<AppUser>
            {
                new AppUser { Id = 1, UserName = "user1", Name = "User One" },
                new AppUser { Id = 2, UserName = "user2", Name = "User Two" }
            };

            var mappedUsers = new List<SummerizedUserWithRateDto>
            {
                new SummerizedUserWithRateDto { Id = 1, Username = "user1", Name = "User One", Rate = 4.5 },
                new SummerizedUserWithRateDto { Id = 2, Username = "user2", Name = "User Two", Rate = 3.8 }
            };

            _mockUnitOfWork.Setup(uow => uow.AppUsers.GetAllAsync(pageNo, pageSize, null, 0))
                           .ReturnsAsync(usersFromDb.AsEnumerable());
            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<SummerizedUserWithRateDto>>(It.IsAny<IEnumerable<AppUser>>()))
                       .Returns(mappedUsers);

            // Act
            var result = await _userService.FindUsersAsync(null, pageNo, pageSize);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.OK));
            Assert.That(result.Data.Data, Is.Not.Null);
            Assert.That(result.Data.Data.Count(), Is.EqualTo(2));
            Assert.That(result.Data.Data.First().Username, Is.EqualTo("user1"));
        }
        [Test]
        public async Task FindUsersAsync_ValidPaginationWithSearchKey_ReturnsFilteredUsers()
        {
            // Arrange
            var searchKey = "user1";
            var pageNo = 1;
            var pageSize = 10;
            var usersFromDb = new List<AppUser>
            {
                new AppUser { Id = 1, UserName = "user1", Name = "User One" }
            };

            var mappedUsers = new List<SummerizedUserWithRateDto>
            {
                new SummerizedUserWithRateDto { Id = 1, Username = "user1", Name = "User One", Rate = 4.5 }
            };

            Expression<Func<AppUser, bool>> expectedCriteria = u =>
                u.UserName.Contains(searchKey) || u.Name.Contains(searchKey);

            _mockUnitOfWork.Setup(uow => uow.AppUsers.FindAsync(
                    It.Is<Expression<Func<AppUser, bool>>>(criteria => criteria.ToString() == expectedCriteria.ToString()),
                    pageNo,
                    pageSize, 
                    null,
                    null,
                    0,
                    false))
                   .ReturnsAsync(usersFromDb.AsEnumerable());
            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<SummerizedUserWithRateDto>>(It.IsAny<IEnumerable<AppUser>>()))
                       .Returns(mappedUsers);

            // Act
            var result = await _userService.FindUsersAsync(searchKey, pageNo, pageSize);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.OK));
            Assert.That(result.Data.Data, Is.Not.Null);
            Assert.That(result.Data.Data.Count(), Is.EqualTo(1));
            Assert.That(result.Data.Data.First().Username, Is.EqualTo("user1"));
        }
        [Test]
        public async Task FindUsersAsync_NoResultsForSearchKey_ReturnsEmptyData()
        {
            // Arrange
            var searchKey = "nonexistent";
            var pageNo = 1;
            var pageSize = 10;

            _mockUnitOfWork.Setup(uow => uow.AppUsers.FindAsync(
                    null,
                    pageNo,
                    pageSize,
                    null,
                    null,
                    0,
                    false))
                   .ReturnsAsync(Enumerable.Empty<AppUser>());

            // Act
            var result = await _userService.FindUsersAsync(searchKey, pageNo, pageSize);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.OK));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Data.Count(), Is.EqualTo(0));
        }

    }
}
