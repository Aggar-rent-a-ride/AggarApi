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
            var result = await _notificationService.AcknowledgeAsync(notificationIds, userId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotificationsAsync(int pageNo, int pageSize)
        {
            var userId = UserHelpers.GetUserId(User);
            var result = await _notificationService.GetNotificationsAsync(userId, pageNo, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}
