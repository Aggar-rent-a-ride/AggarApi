using CORE.Constants;
using CORE.DTOs.AppUser;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> FindUsersAsync(
            [FromQuery] string? searchKey,
            [FromQuery] int pageNo = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.FindUsersAsync(searchKey, pageNo, pageSize);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteUserAsync([FromQuery] int userId)
        {
            var authUserId = UserHelpers.GetUserId(User);
            var roles = UserHelpers.GetUserRoles(User);
            var result = await _userService.DeleteUserAsync(userId, authUserId, roles == null ? new string[] { } : roles.ToArray());
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("punish")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> PunishUserAsync([FromBody] PunishUserDto dto)
        {
            var result = await _userService.PunishUserAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("all")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetTotalUsersAsync(
            [FromQuery] string? role,
            [FromQuery] int pageNo,
            [FromQuery] int pageSize,
            [FromQuery] DateRangePreset? dateFilter)
        {
            var result = await _userService.GetTotalUsersAsync(role, pageNo, pageSize, dateFilter);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("count-all")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetTotalUsersAsync([FromQuery] string? role)
        {
            var result = await _userService.GetTotalUsersCountAsync(role);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] int userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
