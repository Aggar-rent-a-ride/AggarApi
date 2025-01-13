using AutoMapper;
using CORE.Constants;
using CORE.DTOs.Auth;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CORE.Services
{
    public class AuthServiceTests
    {
        private Mock<UserManager<AppUser>> _mockUserManager;
        private Mock<IOptions<JwtConfig>> _mockJwtOptions;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IMapper> _mockMapper;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IMemoryCache> _mockMemoryCache;
        private AuthService _authService;
        private Mock<UserManager<AppUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<AppUser>>();
            return new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [SetUp]
        public void SetUp()
        {
            _mockUserManager = MockUserManager();
            _mockJwtOptions = new Mock<IOptions<JwtConfig>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEmailService = new Mock<IEmailService>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _authService = new AuthService(
                _mockJwtOptions.Object,
                _mockUserManager.Object,
                _mockConfiguration.Object,
                _mockMapper.Object,
                _mockUnitOfWork.Object,
                _mockEmailService.Object,
                _mockMemoryCache.Object
            );
        }
        [Test]
        public async Task RegisterAsync_ShouldReturnValidationMessage_WhenTermsNotAgreed()
        {
            // Arrange
            var registerDto = new RegisterDto { AggreedTheTerms = false };

            // Act
            var result = await _authService.RegisterAsync(registerDto, null);

            // Assert
            Assert.That(result.IsAuthenticated == false);
            Assert.That(result.Message == "You must agree to the Terms and Conditions to register");
        }
        [Test]
        public async Task RegisterAsync_ShouldReturnValidationMessage_WhenUsernameExists()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "ExistingUser", AggreedTheTerms = true };
            _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
                            .ReturnsAsync(new AppUser());

            // Act
            var result = await _authService.RegisterAsync(registerDto, null);

            // Assert
            Assert.That(result.IsAuthenticated == false);
            Assert.That(result.Message == "Username already exists");
        }
        [Test]
        public async Task RegisterAsync_ShouldReturnValidationMessage_WhenEmailExists()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "User", Email = "ExistingEmail", AggreedTheTerms = true };
            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync(new AppUser());

            // Act
            var result = await _authService.RegisterAsync(registerDto, null);

            // Assert
            Assert.That(result.IsAuthenticated == false);
            Assert.That(result.Message == "Email already exists");
        }
        [Test]
        public async Task RegisterAsync_ShouldReturnMessage_WhenUserCreationFails()
        {
            // Arrange
            var registerDto = new RegisterDto { AggreedTheTerms = true, IsCustomer = true };
            _mockMapper.Setup(m => m.Map<Customer>(registerDto))
                       .Returns(new Customer());

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            // Act
            var result = await _authService.RegisterAsync(registerDto, null);

            // Assert
            Assert.That(result.IsAuthenticated == false);
            Assert.That(result.Message == "Password too weak");
        }
        [Test]
        public async Task RegisterAsync_ShouldReturnMessage_WhenRoleAssignmentFails()
        {
            // Arrange
            var registerDto = new RegisterDto { AggreedTheTerms = true, IsCustomer = true };
            var roles = new List<string> { "Admin" };

            _mockMapper.Setup(m => m.Map<Customer>(registerDto))
                       .Returns(new Customer());

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<AppUser>(), roles))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role does not exist" }));

            // Act
            var result = await _authService.RegisterAsync(registerDto, roles);

            // Assert
            Assert.That(result.IsAuthenticated == false);
            Assert.That(result.Message == "User created, but roles couldn't be assigned: Role does not exist");
        }
        [Test]
        public async Task RegisterAsync_ShouldReturnSuccessMessage_WhenRegistrationSucceeds()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                AggreedTheTerms = true,
                IsCustomer = true,
                Username = "NewUser",
                Email = "newuser@email.com"
            };

            var roles = new List<string> { "Customer" };

            _mockMapper.Setup(m => m.Map<Customer>(registerDto))
                       .Returns(new Customer());

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<AppUser>(), roles))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>()))
                            .ReturnsAsync(roles);

            // Act
            var result = await _authService.RegisterAsync(registerDto, roles);

            // Assert
            Assert.That(result.IsAuthenticated == true);
            Assert.That(result.Message == "Registered Successfully");
            Assert.That(result.Roles, Is.EquivalentTo(roles));
            Assert.That(result.Username == registerDto.Username);
            Assert.That(result.Email == registerDto.Email);
        }
        [Test]
        public async Task LoginAsync_ShouldHandleUnexpectedStatusValues()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "testuser", Password = "password" };

            var user = new AppUser { UserName = "testuser", Email = "test@example.com", Id = 123, Status = (UserStatus)999 }; // An undefined status

            _mockUserManager.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, "password")).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.That(result != null);
            Assert.That(result.IsAuthenticated == true);
            Assert.That("Your account status is undefined" == result.Message); // Assuming `GetUserStatusMessage` handles this edge case
        }
        [Test]
        public async Task LoginAsync_UserHasNoRoles_ShouldReturnMessage()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "testuser", Password = "password" };
            var user = new AppUser { UserName = "testuser", Email = "test@example.com", Id = 123, Status = UserStatus.Active }; // An undefined status
            _mockUserManager.Setup(m => m.FindByNameAsync("testuser")).ReturnsAsync(user);
            _mockUserManager.Setup(m=>m.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _mockUserManager.Setup(m=>m.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.That(result != null);
            Assert.That(result.Message == "User has no roles, Try logging in again");
        }
        [Test]
        public async Task SendActivationCodeAsync_UserIsNull()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.Get(It.IsAny<int>())).ReturnsAsync((AppUser) null);

            // Act 
            var result = await _authService.SendActivationCodeAsync(5);

            // Assert
            Assert.That(result.StatusCode == StatusCodes.NotFound);
        }
    }
}
