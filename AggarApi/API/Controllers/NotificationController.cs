using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpPut("ack")]
        [Authorize]
        public async Task<IActionResult> Acknowledge([FromBody] HashSet<int> notificationIds)
        {
            var userId = UserHelpers.GetUserId(User);
            var result = await _notificationService.Acknowledge(notificationIds, userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
