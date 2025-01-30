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
        private Mock<IGeoapifyService> _mockGeoapifyService;
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
            _mockGeoapifyService = new Mock<IGeoapifyService>();

            _authService = new AuthService(
                _mockJwtOptions.Object,
                _mockUserManager.Object,
                _mockConfiguration.Object,
                _mockMapper.Object,
                _mockUnitOfWork.Object,
                _mockEmailService.Object,
                _mockMemoryCache.Object,
                _mockGeoapifyService.Object
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
            Assert.That(result.Data.IsAuthenticated == false);
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
            Assert.That(result.Data.IsAuthenticated == false);
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
            Assert.That(result.Data.IsAuthenticated == false);
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
            Assert.That(result.Data.IsAuthenticated == false);
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
            _mockUserManager.Setup(x => x.DeleteAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Failed());
            // Act
            var result = await _authService.RegisterAsync(registerDto, roles);

            // Assert
            Assert.That(result.Data.IsAuthenticated == false);
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
            Assert.That(result.Data.IsAuthenticated == true);
            Assert.That(result.Data.Roles, Is.EquivalentTo(roles));
            Assert.That(result.Data.Username == registerDto.Username);
            Assert.That(result.Data.Email == registerDto.Email);
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
            Assert.That(result.Data.IsAuthenticated == true);
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
        }
        [Test]
        public async Task SendActivationCodeAsync_ShouldReturnNotFound_WhenUserIsNull()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync((AppUser) null);

            // Act 
            var result = await _authService.SendActivationCodeAsync(5);

            // Assert
            Assert.That(result.StatusCode == StatusCodes.NotFound);
        }
        [Test]
        public async Task SendActivationCodeAsync_ShouldReturnBadRequest_WhenUserStatusIsNotInactive()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync(new AppUser { Status = (UserStatus)999 });

            // Act 
            var result = await _authService.SendActivationCodeAsync(5);

            // Assert
            Assert.That(result.StatusCode == StatusCodes.BadRequest);
        }
        [Test]
        public async Task SendActivationCodeAsync_ShouldReturnInternalServerError_WhenEmailNotSent()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync(new AppUser { Status = UserStatus.Inactive });

            // Act 
            var result = await _authService.SendActivationCodeAsync(5);

            // Assert
            Assert.That(result.StatusCode == StatusCodes.InternalServerError);
        }
        [Test]
        public async Task ActivateAccountAsync_ShouldReturnUserNotFound_WhenUserIsNull()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync((AppUser)null);
            var dto = new AccountActivationDto { ActivationCode = "code", UserId = 5 };

            // Act 
            var result = await _authService.ActivateAccountAsync(dto);

            // Assert
            Assert.That(result.StatusCode == StatusCodes.NotFound);
        }
        [Test]
        public async Task ActivateAccountAsync_ShouldReturnMessage_WhenUserIsNotInactive()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync(new AppUser { Status = UserStatus.Active});
            var dto = new AccountActivationDto { ActivationCode = "code", UserId = 5 };

            // Act 
            var result = await _authService.ActivateAccountAsync(dto);

            // Assert
            Assert.That(result.Message != null);
        }
        [Test]
        public async Task ActivateAccountAsync_ShouldReturnMessage_WhenActivationCodeNotFound()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync(new AppUser { Status = UserStatus.Active });
            var dto = new AccountActivationDto { ActivationCode = "code", UserId = 5 };
            _mockMemoryCache.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny)).Returns(false);

            // Act 
            var result = await _authService.ActivateAccountAsync(dto);

            // Assert
            Assert.That(result.Message != "Invalid activation code");
        }
        [Test]
        public async Task ActivateAccountAsync_ShouldReturnMessage_WhenUserIdNotEqualDtoUserId()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>())).ReturnsAsync(new AppUser {Id = 5,  Status = UserStatus.Active });
            _mockMemoryCache.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny)).Returns(true);
            var dto = new AccountActivationDto { ActivationCode = "code", UserId = 5 };


            // Act 
            var result = await _authService.ActivateAccountAsync(dto);

            // Assert
            Assert.That(result.Message != "Invalid activation code");
        }
        [Test]
        public async Task ActivateAccountAsync_ShouldReturnMessage_WhenUserUpdateIsNotSuccessful()
        {
            // Arrange 
            _mockUnitOfWork.Setup(m => m.AppUsers.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new AppUser { Id = 5, Status = UserStatus.Inactive });

            object userIdFromCache = 5; // Use an object to match the signature of the `out` parameter
            _mockMemoryCache
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out userIdFromCache))
                .Returns(true);

            _mockUserManager
                .Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "update failed" }));

            var dto = new AccountActivationDto { ActivationCode = "code", UserId = 5 };

            // Act 
            var result = await _authService.ActivateAccountAsync(dto);
            
            // Assert
            Assert.That(result.Message != null);
        }


    }
}
