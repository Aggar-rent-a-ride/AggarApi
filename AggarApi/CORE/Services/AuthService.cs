using AutoMapper;
using CORE.DTOs.Auth;
using CORE.Services.IServices;
using DATA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DATA.Models.Enums;
using CORE.Constants;
using RTools_NTS.Util;
using CORE.DTOs;
using DATA.DataAccess.Repositories.UnitOfWork;
using CORE.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace CORE.Services
{
    public class AuthService : IAuthService
    {
        private readonly IOptions<JwtConfig> _jwt;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;

        public AuthService(IOptions<JwtConfig> jwt,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IMemoryCache memoryCache)
        {
            _jwt = jwt;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }
        private async Task<string?> ValidateRegistrationAsync(RegisterDto registerDto)
        {
            if (!registerDto.AggreedTheTerms)
                return "You must agree to the Terms and Conditions to register";
            if (await _userManager.FindByNameAsync(registerDto.Username) != null)
                return "Username already exists";
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return "Email already exists";
            return null;
        }
        private async Task<string> CreateAccessTokenAsync(AppUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleClaims = userRoles.Select(role => new Claim("roles", role)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, $"{user.Id}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.UserName)
            }
            .Union(roleClaims);

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Value.Key));
            var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                    issuer: _jwt.Value.Issuer,
                    audience: _jwt.Value.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(_jwt.Value.DurationInHours),
                    signingCredentials: signingCredentials
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = RandomNumberGenerator.GetBytes(32);
            var rawToken = Convert.ToBase64String(randomNumber);

            using (var sha256 = SHA256.Create())
            {
                var hashedToken = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken)));

                return new RefreshToken
                {
                    Token = hashedToken, // Store hashed token in the database
                    RawToken = rawToken, // Send raw token back to the client
                    ExpiresOn = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("RefreshTokenDurationInDays")),
                    CreatedOn = DateTime.UtcNow,
                };
            }
        }
        private string GetUserStatusMessage(UserStatus status)
        {
            if (status == UserStatus.Inactive || status == UserStatus.Banned || status == UserStatus.Removed)
                return $"Your account is {status.ToString().ToLower()}";
            else if (status == UserStatus.Active)
                return null;
            return $"Your account status is undefined";
        }
        public static string GenerateActivationCode(int length = 6)
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder(length);
            byte[] randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            foreach (byte randomByte in randomBytes)
            {
                result.Append(characters[randomByte % characters.Length]);
            }

            return result.ToString();
        }
        private async Task AddRefreshToken(AppUser user, RefreshToken refreshToken)
        {
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }
        private async Task<bool> ValidateRefreshTokenAsync(string providedToken, AppUser user)
        {
            var refreshToken = user.RefreshTokens.FirstOrDefault(r => r.IsActive);
            if (refreshToken == null) return false;

            // Hash the provided token
            using (var sha256 = SHA256.Create())
            {
                var hashedProvidedToken = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(providedToken)));
                return hashedProvidedToken == refreshToken.Token;
            }
        }
        private async Task<RefreshToken> ProcessUserRefreshToken(AppUser user)
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(r => r.IsActive);
            if (activeRefreshToken != null)
                activeRefreshToken.RevokedOn = DateTime.UtcNow;
            var refreshToken = GenerateRefreshToken();
            await AddRefreshToken(user, refreshToken);
            return refreshToken;
        }
        public async Task<AuthDto> RegisterAsync(RegisterDto registerDto, List<string> roles)
        {
            if (await ValidateRegistrationAsync(registerDto) is string validationMessage)
            {
                return new AuthDto { Message = validationMessage };
            }

            AppUser user = registerDto.IsCustomer ? _mapper.Map<Customer>(registerDto) : _mapper.Map<Renter>(registerDto);
            
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded == false)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthDto { Message = errors };
            }

            if (roles != null && roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, roles);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return new AuthDto { Message = $"User created, but roles couldn't be assigned: {roleErrors}" };
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            return new AuthDto
            {
                UserId = user.Id,
                IsAuthenticated = true,
                Message = "Registered Successfully",
                Roles = userRoles.ToList(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                AccountStatus = user.Status.ToString().ToLower()
            };
        }
        public async Task<AuthDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UsernameOrEmail);
            if (user == null)
                user = await _userManager.FindByEmailAsync(loginDto.UsernameOrEmail);

            if (user == null || await _userManager.CheckPasswordAsync(user, loginDto.Password) == false)
                return new AuthDto { Message = "Username or password is incorrect" };

            var roles = await _userManager.GetRolesAsync(user);
            if(roles == null || roles.Count == 0)
                return new AuthDto { Message = "User has no roles, Try logging in again" };

            var authDto = new AuthDto
            {
                UserId = user.Id,
                IsAuthenticated = true,
                Username = user?.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                AccountStatus = user.Status.ToString().ToLower(),
            };

            if (GetUserStatusMessage(user.Status) is string statusMessage)
            {
                authDto.Message = statusMessage;
                return authDto;
            }

            authDto.AccessToken = await CreateAccessTokenAsync(user);

            var refreshToken = await ProcessUserRefreshToken(user);
            authDto.RefreshToken = refreshToken.RawToken;
            authDto.RefreshTokenExpiration = refreshToken.ExpiresOn;

            return authDto;
        }
        public async Task<ResponseDto<object>> SendActivationCodeAsync(int userId)
        {
            var user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
                return new ResponseDto<object> { Message = "User not found", StatusCode = StatusCodes.NotFound };
            if (user.Status != UserStatus.Inactive)
                return new ResponseDto<object> { Message = $"Your account is {user.Status.ToString().ToLower()}", StatusCode = StatusCodes.BadRequest };

            var activationCode = GenerateActivationCode();
            var emailSent = await _emailService.SendEmailAsync(user.Email, "Activate your account", HtmlHelpers.GenerateAccountActivationHtmlBody(activationCode));
            if (emailSent == false)
                return new ResponseDto<object> { Message = "Something went wrong. Please try again", StatusCode =  StatusCodes.InternalServerError };

            _memoryCache.Set(activationCode, userId, TimeSpan.FromMinutes(5));

            return new ResponseDto<object> { StatusCode = StatusCodes.OK };
        }
        public async Task<AuthDto> ActivateAccountAsync(AccountActivationDto dto)
        {
            var user = await _unitOfWork.AppUsers.GetAsync(dto.UserId);
            if (user == null)
                return new AuthDto { Message = "User not found" };
            if (user.Status != UserStatus.Inactive)
                return new AuthDto { Message = $"Your account is {user.Status.ToString().ToLower()}" };

            if(_memoryCache.TryGetValue(dto.ActivationCode, out int userId) == false || userId != user.Id)
                return new AuthDto { Message = "Invalid activation code" };

            _memoryCache.Remove(dto.ActivationCode);

            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            var updateUserResult = await _userManager.UpdateAsync(user);
            if(updateUserResult.Succeeded == false)
            {
                var errors = string.Join(", ", updateUserResult.Errors.Select(e => e.Description));
                return new AuthDto { Message = $"Account couldn't be activated: {errors}" };
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Count == 0)
                return new AuthDto { Message = "User has no roles, Try logging in again" };

            return new AuthDto
            {
                UserId = user.Id,
                IsAuthenticated = true,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                AccountStatus = user.Status.ToString().ToLower(),
                Message = "Your account has been activated successfully. Try to login to access your account"
            };
        }
    }
}
