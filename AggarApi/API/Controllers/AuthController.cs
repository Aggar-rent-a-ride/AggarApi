using CORE.Constants;
using CORE.DTOs.Auth;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDto registerDto)
        {
            var roles = new List<string> { Roles.User };
            if (registerDto.IsCustomer == true)
                roles.Add(Roles.Customer);
            else
                roles.Add(Roles.Renter);
            var result = await _authService.RegisterAsync(registerDto, roles);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("sendActivationCode{userId}")]
        public async Task<IActionResult> SendActivationCodeAsync(int userId)
        {
            var result = await _authService.SendActivationCodeAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAccountAsync(AccountActivationDto dto)
        {
            var result = await _authService.ActivateAccountAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessTokenAsync(RefreshTokenDto dto)
        {
            var result = await _authService.RefreshAccessTokenAsync(dto.RefreshToken);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> RevokeRefreshTokenAsync(RefreshTokenDto dto)
        {
            var result = await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdminAsync(RegisterAdminDto dto)
        {
            var roles = new List<string> { Roles.Admin };
            var result = await _authService.UpdateUserRolesAsync(dto.UserId, roles);
            return StatusCode(result.StatusCode, result);
        }
    }
}
