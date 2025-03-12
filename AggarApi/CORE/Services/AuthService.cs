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
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore;
using DATA.Constants;
using Microsoft.Extensions.Logging;
using Serilog;

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
        private readonly IGeoapifyService _geoapifyService;
        private readonly IEmailTemplateRendererService _emailTemplateRendererService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IOptions<JwtConfig> jwt,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IMemoryCache memoryCache,
            IGeoapifyService geoapifyService,
            IEmailTemplateRendererService emailTemplateRendererService,
            ILogger<AuthService> logger)
        {
            _jwt = jwt;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _memoryCache = memoryCache;
            _geoapifyService = geoapifyService;
            _emailTemplateRendererService = emailTemplateRendererService;
            _logger = logger;
        }
        private async Task<string?> ValidateRegistrationAsync(RegisterDto registerDto)
        {
            if (!registerDto.AggreedTheTerms)
                return "You must agree to the Terms and Conditions to register.";
            if (await _userManager.FindByNameAsync(registerDto.Username) != null)
                return "Username already exists.";
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return "Email already exists.";
            return null;
        }
        private async Task<IdentityResult> UpdateUserRolesByUserAsync(AppUser user, List<string> roles)
        {
            return await _userManager.AddToRolesAsync(user, roles);
        }
        private async Task<string> CreateAccessTokenAsync(AppUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleClaims = userRoles.Select(role => new Claim("roles", role)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, $"{user.Id}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.UserName),
                new Claim("uid", user.Id.ToString()),
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
            if (status == UserStatus.Inactive || status == UserStatus.Banned)
                return $"Your account is {status.ToString().ToLower()}.";
            else if (status == UserStatus.Active)
                return null;
            return $"Your account status is undefined.";
        }
        private static string GenerateActivationCode(int length = 6)
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
        private string HashRefreshToken(string rawToken)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedToken = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken)));
                return hashedToken;
            }
        }
        private RefreshToken? ValidateRefreshToken(string providedToken, AppUser user)
        {
            var refreshToken = user.RefreshTokens.FirstOrDefault(r => r.IsActive);
            if (refreshToken == null) return null;

            return HashRefreshToken(providedToken) == refreshToken.Token ? refreshToken : null;
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
        public async Task<ResponseDto<AuthDto>> RegisterAsync(RegisterDto registerDto, List<string> roles)
        {
            _logger.LogInformation($"Starting user registration process.");

            if (await ValidateRegistrationAsync(registerDto) is string validationMessage)
            {
                _logger.LogWarning("User registration failed. {validationMessage}", validationMessage);
                return new ResponseDto<AuthDto> {  StatusCode = StatusCodes.BadRequest, Message = validationMessage };
            }

            AppUser user = registerDto.IsCustomer ? 
                _mapper.Map<Customer>(registerDto) : 
                _mapper.Map<Renter>(registerDto);

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded == false)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user. {errors}", errors);
                return new ResponseDto<AuthDto> {StatusCode = StatusCodes.InternalServerError, Message = errors };
            }

            _logger.LogInformation("User created successfully. UserId: {userId}", user.Id);

            if (roles != null && roles.Any() == true)
            {
                _logger.LogDebug("Attempting to assign roles {Roles} to user {UserId}",
                    string.Join(", ", roles), user.Id);

                var roleResult = await UpdateUserRolesByUserAsync(user, roles);
                if (roleResult.Succeeded == false)
                {
                    var errors = "";
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    errors += $"Failed to assign roles: {roleErrors}.";

                    _logger.LogError("Role assignment failed for user {UserId}. Errors: {RoleErrors}",
                        user.Id, roleErrors);

                    var deleteResult = await _userManager.DeleteAsync(user);
                    if (deleteResult.Succeeded == false)
                    {
                        var deleteErrors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                        errors += $"\nFailed to cleanup user: {deleteErrors}.";

                        _logger.LogError("Failed to delete user {UserId} after role assignment failure. Errors: {DeleteErrors}",
                            user.Id, deleteErrors);
                    }
                    else
                    {
                        _logger.LogInformation("User {UserId} was deleted after role assignment failure", user.Id);
                    }
                    return new ResponseDto<AuthDto> { StatusCode = StatusCodes.InternalServerError, Message = errors };
                }
                _logger.LogInformation("Roles {Roles} successfully assigned to user {UserId}",
                    string.Join(", ", roles), user.Id);
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("User registration completed successfully. UserID: {UserId}, Roles: {UserRoles}",
                user.Id, string.Join(", ", userRoles));

            return new ResponseDto<AuthDto>
            {
                Data = new AuthDto
                {
                    UserId = user.Id,
                    IsAuthenticated = true,
                    Roles = userRoles.ToList(),
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    AccountStatus = user.Status.ToString().ToLower()
                },
                StatusCode = StatusCodes.Created,
                Message = "Registered Successfully.",
            };
        }
        public async Task<ResponseDto<AuthDto>> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for username/email: {UsernameOrEmail}", loginDto.UsernameOrEmail);

            var user = await _userManager.FindByNameAsync(loginDto.UsernameOrEmail);
            if (user == null)
            {
                _logger.LogDebug("User not found by username, trying email lookup: {UsernameOrEmail}", loginDto.UsernameOrEmail);
                user = await _userManager.FindByEmailAsync(loginDto.UsernameOrEmail); 
            }

            if (user == null || await _userManager.CheckPasswordAsync(user, loginDto.Password) == false)
            {
                _logger.LogWarning("Login failed: User not found for {UsernameOrEmail} or for invalid password {loginDto.Password}", loginDto.UsernameOrEmail, loginDto.Password);
                return new ResponseDto<AuthDto> { StatusCode = StatusCodes.BadRequest, Message = "Username or password is incorrect." };
            }

            _logger.LogDebug("Password validation successful for user {UserId}", user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            if(roles == null || roles.Count == 0)
            {
                _logger.LogError("User {UserId} has no assigned roles", user.Id);
                return new ResponseDto<AuthDto> { StatusCode = StatusCodes.InternalServerError, Message = "User has no roles, Try logging in again." };
            }

            _logger.LogDebug("Retrieved roles for user {UserId}: {Roles}", user.Id, string.Join(", ", roles));

            var result = new ResponseDto<AuthDto>
            {
                Data = new AuthDto
                {
                    UserId = user.Id,
                    IsAuthenticated = true,
                    Username = user?.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    AccountStatus = user.Status.ToString().ToLower(),
                },
                StatusCode = StatusCodes.OK,
                Message = "Logged in successfully."
            };

            if (GetUserStatusMessage(user.Status) is string statusMessage)
            {
                _logger.LogWarning("Login restricted due to account status: {UserId}, {Username}, Status: {Status}",
                    user.Id, user.UserName, user.Status);
                result.Message = statusMessage;
                result.StatusCode = StatusCodes.BadRequest;
                return result;
            }

            _logger.LogDebug("Generating access token for user {UserId}", user.Id);
            result.Data.AccessToken = await CreateAccessTokenAsync(user);

            _logger.LogDebug("Processing refresh token for user {UserId}", user.Id);
            var refreshToken = await ProcessUserRefreshToken(user);
            result.Data.RefreshToken = refreshToken.RawToken;
            result.Data.RefreshTokenExpiration = refreshToken.ExpiresOn;

            _logger.LogInformation("Login successful for user {UserId}, {Username}", user.Id, user.UserName);

            return result;
        }
        public async Task<ResponseDto<object>> SendActivationCodeAsync(int userId)
        {
            _logger.LogInformation("Sending activation code for user ID: {UserId}", userId);

            var user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Activation code request failed: User not found. User ID: {UserId}", userId);
                return new ResponseDto<object> { Message = "User not found.", StatusCode = StatusCodes.NotFound };
            }

            _logger.LogDebug("User found: {UserId}, Email: {Email}, Status: {Status}",
                userId, user.Email, user.Status);

            if (user.Status != UserStatus.Inactive)
            {
                _logger.LogWarning("Activation code not sent: User status is not Inactive. User ID: {UserId}, Current Status: {Status}",
                    userId, user.Status);
                return new ResponseDto<object> { Message = $"Your account is {user.Status.ToString().ToLower()}.", StatusCode = StatusCodes.BadRequest };
            }

            var activationCode = GenerateActivationCode();
            _logger.LogDebug("Generated activation code for user {UserId}", userId);

            var emailSent = await _emailService.SendEmailAsync(user.Email, EmailSubject.ActivateYourAccount, await _emailTemplateRendererService.RenderTemplateAsync(Templates.AccountActivationEmail, new { Code = System.Web.HttpUtility.HtmlEncode(activationCode) }));
            if (emailSent == false)
            {
                _logger.LogError("Failed to send activation email to user {UserId} at {Email}",
                    userId, user.Email);
                return new ResponseDto<object> { Message = "Something went wrong. Please try again.", StatusCode = StatusCodes.InternalServerError };
            }
            _logger.LogDebug("Activation email sent successfully to user {UserId}", userId);

            _memoryCache.Set(activationCode, userId, TimeSpan.FromMinutes(5));
            _logger.LogDebug("Activation code cached for user {UserId} with 5-minute expiration", userId);


            _logger.LogInformation("Activation process completed successfully for user {UserId}", userId);
            return new ResponseDto<object> { StatusCode = StatusCodes.OK };
        }
        public async Task<ResponseDto<AuthDto>> ActivateAccountAsync(AccountActivationDto dto)
        {
            _logger.LogInformation("Account activation attempt for user ID: {UserId}", dto.UserId);

            var user = await _unitOfWork.AppUsers.GetAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("Account activation failed: User not found. User ID: {UserId}", dto.UserId);
                return new ResponseDto<AuthDto> { Message = "User not found.", StatusCode = StatusCodes.NotFound };
            }

            _logger.LogDebug("User found for activation: {UserId}, Email: {Email}, Status: {Status}",
                dto.UserId, user.Email, user.Status);

            if (user.Status != UserStatus.Inactive)
            {
                _logger.LogWarning("Account activation failed: User status is not Inactive. User ID: {UserId}, Current Status: {Status}",
                    dto.UserId, user.Status);
                return new ResponseDto<AuthDto> { Message = $"Your account is {user.Status.ToString().ToLower()}.", StatusCode = StatusCodes.BadRequest };
            }

            // Not logging the activation code itself for security
            bool codeValid = _memoryCache.TryGetValue(dto.ActivationCode, out int userId);
            bool userMatches = userId == user.Id;

            if (codeValid == false || userMatches == false)
            {
                _logger.LogWarning("Account activation failed: Invalid activation code for user {UserId}. Code found: {CodeFound}, User matched: {UserMatched}",
                    dto.UserId, codeValid, userMatches);
                return new ResponseDto<AuthDto> { Message = "Invalid activation code.", StatusCode = StatusCodes.BadRequest };
            }
            _logger.LogDebug("Activation code validated for user {UserId}", dto.UserId);

            _memoryCache.Remove(dto.ActivationCode);
            _logger.LogDebug("Activation code removed from cache for user {UserId}", dto.UserId);


            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            _logger.LogDebug("Updating user {UserId} status to Active and confirming email", dto.UserId);

            var updateUserResult = await _userManager.UpdateAsync(user);
            if(updateUserResult.Succeeded == false)
            {
                var errors = string.Join(", ", updateUserResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update user {UserId} status. Errors: {Errors}",
                    dto.UserId, errors);
                return new ResponseDto<AuthDto> {   Message = $"Account couldn't be activated: {errors}.", StatusCode = StatusCodes.InternalServerError };
            }

            _logger.LogDebug("User {UserId} status updated successfully", dto.UserId);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Count == 0)
            {
                _logger.LogError("User {UserId} has no assigned roles after activation", dto.UserId);
                return new ResponseDto<AuthDto> { Message = "User has no roles. Try logging in again.", StatusCode = StatusCodes.InternalServerError };
            }

            _logger.LogDebug("Retrieved roles for user {UserId}: {Roles}",
                dto.UserId, string.Join(", ", roles));

            _logger.LogInformation("Account activation completed successfully for user {UserId}", dto.UserId);

            return new ResponseDto<AuthDto>
            {
                Data = new AuthDto
                {
                    UserId = user.Id,
                    IsAuthenticated = true,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    AccountStatus = user.Status.ToString().ToLower(),
                },
                StatusCode = StatusCodes.OK,
                Message = "Your account has been activated successfully. Try to login to access your account."
            };
        }
        public async Task<ResponseDto<AuthDto>> RefreshAccessTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Attempting to refresh access token");

            var hashedRefreshToken = HashRefreshToken(refreshToken);
            _logger.LogDebug("Refresh token hashed, searching for user with matching token");

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == hashedRefreshToken));
            if (user == null)
            {
                _logger.LogWarning("No user found with the provided refresh token");
                return new ResponseDto<AuthDto> { Message = "Invalid token.", StatusCode = StatusCodes.BadRequest };
            }

            _logger.LogDebug("User found: {Username}. Validating refresh token", user.UserName);

            if (ValidateRefreshToken(refreshToken, user) == null)
            {
                _logger.LogWarning("Refresh token validation failed for user: {Username}", user.UserName);
                return new ResponseDto<AuthDto> { Message = "Invalid token.", StatusCode = StatusCodes.BadRequest };
            }

            _logger.LogDebug("Refresh token validated successfully. Retrieving user roles");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Any() == false)
            {
                _logger.LogError("User {Username} has no roles assigned", user.UserName);
                return new ResponseDto<AuthDto> { Message = "User has no roles assigned.", StatusCode = StatusCodes.InternalServerError };
            }

            _logger.LogDebug("User roles retrieved successfully. Processing new refresh token");

            var newRefreshToken = await ProcessUserRefreshToken(user);

            _logger.LogDebug("Creating new access token");

            var accessToken = await CreateAccessTokenAsync(user);

            _logger.LogInformation("Access token refreshed successfully for user: {Username}", user.UserName);

            return new ResponseDto<AuthDto>
            {
                Data = new AuthDto
                {
                    IsAuthenticated = true,
                    AccessToken = accessToken,
                    Username = user.UserName,
                    Email = user.Email,
                    RefreshToken = newRefreshToken.RawToken,
                    RefreshTokenExpiration = newRefreshToken.ExpiresOn,
                    Roles = roles.ToList()
                },
                StatusCode = StatusCodes.OK,
            };
        }
        public async Task<ResponseDto<object>> RevokeRefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Revoking refresh token.");

            var hashedRefreshToken = HashRefreshToken(refreshToken);
            _logger.LogDebug("Hashed refresh token: {HashedToken}", hashedRefreshToken);

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == hashedRefreshToken));

            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token provided.");
                return new ResponseDto<object> { Message = "Invalid token.", StatusCode = StatusCodes.BadRequest };
            }

            _logger.LogInformation("User found for the given refresh token. Validating the token.");
            var token = ValidateRefreshToken(refreshToken, user);

            if (token == null)
            {
                _logger.LogWarning("Refresh token validation failed.");
                return new ResponseDto<object> { Message = "Invalid token.", StatusCode = StatusCodes.BadRequest };
            }

            _logger.LogInformation("Token validation successful. Revoking token.");
            token.RevokedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded == false)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to revoke refresh token: {Errors}", errors);
                return new ResponseDto<object> { Message = $"Failed to revoke token: {errors}.", StatusCode = StatusCodes.InternalServerError };
            }

            _logger.LogInformation("Refresh token successfully revoked.");
            return new ResponseDto<object> { StatusCode = StatusCodes.OK, Message = "You have been logged out. Please delete your access token and refresh token." };
        }
        public async Task<ResponseDto<object>> UpdateUserRolesAsync(int userId, List<string> roles)
        {
            _logger.LogInformation("Updating roles for user with ID {UserId}.", userId);

            var user = await _unitOfWork.AppUsers.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return new ResponseDto<object> { Message = "User not found.", StatusCode = StatusCodes.NotFound };
            }

            _logger.LogInformation("User found. Updating roles: {Roles}", string.Join(", ", roles));

            var result = await UpdateUserRolesByUserAsync(user, roles);
            if (result.Succeeded == false)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update roles for user ID {UserId}: {Errors}", userId, errors);
                return new ResponseDto<object> { Message = $"Failed to update roles: {errors}.", StatusCode = StatusCodes.InternalServerError };
            }
            _logger.LogInformation("Roles updated successfully for user ID {UserId}.", userId);
            
            //email notification
            if (roles.Contains(Roles.Admin))
                await _emailService.SendEmailAsync(user.Email, EmailSubject.NotificationReceived, await _emailTemplateRendererService.RenderTemplateAsync(Templates.Notification, new { NotificationContent = System.Web.HttpUtility.HtmlEncode("Your account was assigned to be an Admin"), NotificationType = System.Web.HttpUtility.HtmlEncode(NotificationType.AssignedToAdmin) }));
            
            return new ResponseDto<object> { StatusCode = StatusCodes.OK, Message = "Roles updated successfully." };
        }
    }
}
