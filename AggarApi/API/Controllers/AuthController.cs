using CORE.Constants;
using CORE.DTOs.Auth;
using CORE.Services.IServices;
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
            var roles = new List<string> {Roles.User};
            if (registerDto.IsCustomer == true)
                roles.Add(Roles.Customer);
            else
                roles.Add(Roles.Renter);
            var result = await _authService.RegisterAsync(registerDto, roles);
            if(result.IsAuthenticated == false)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
