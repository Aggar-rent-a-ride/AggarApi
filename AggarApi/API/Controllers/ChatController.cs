using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public ChatController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessagesAsync([FromQuery] int userId, [FromQuery] DateTime dateTime, [FromQuery] int pageSize)
        {
            var currentAuthenticatedUser = UserHelpers.GetUserId(User);
            var result = await _messageService.GetMessagesAsync(currentAuthenticatedUser, userId, dateTime, pageSize);

            return StatusCode(result.StatusCode, result);
        }
    }
}
