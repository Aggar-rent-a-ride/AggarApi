using CORE.Helpers;
using CORE.Services.IServices;
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
            var result = await _userService.DeleteUserAsync(userId, authUserId, roles == null? new string[] { }: roles.ToArray());
            return StatusCode(result.StatusCode, result);
        }
    }
}
