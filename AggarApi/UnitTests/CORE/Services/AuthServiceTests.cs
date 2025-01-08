using AutoMapper;
using CORE.DTOs.Auth;
using CORE.Services;
using DATA.Models;
using Microsoft.AspNetCore.Identity;
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

            _authService = new AuthService(
                _mockJwtOptions.Object,
                _mockUserManager.Object,
                _mockConfiguration.Object,
                _mockMapper.Object
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
            var registerDto = new RegisterDto { Username = "User",  Email = "ExistingEmail", AggreedTheTerms = true };
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
    }
}
