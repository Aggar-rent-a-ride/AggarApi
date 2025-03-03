using CORE.DTOs.Chat;
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
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }
        [Authorize]
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessagesAsync([FromQuery] int userId, [FromQuery] DateTime dateTime, [FromQuery] int pageSize, [FromQuery] DateFilter dateFilter = DateFilter.Before)
        {
            var currentAuthenticatedUser = UserHelpers.GetUserId(User);
            var result = await _chatService.GetMessagesAsync(currentAuthenticatedUser, userId, dateTime, pageSize, dateFilter);

            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpGet("chat")]
        public async Task<IActionResult> GetChatAsync([FromQuery] int pageNo, [FromQuery] int pageSize)
        {
            var currentAuthenticatedUser = UserHelpers.GetUserId(User);
            var result = await _chatService.GetChatAsync(currentAuthenticatedUser, pageNo, pageSize);

            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpPut("ack")]
        public async Task<IActionResult> AcknowledgeMessagesAsync([FromForm] HashSet<int> messageIds)
        {
            var currentAuthenticatedUser = UserHelpers.GetUserId(User);
            var result = await _chatService.AcknowledgeMessagesAsync(currentAuthenticatedUser, messageIds);

            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterMessagesAsync(MessageFilterDto filter)
        {
            var currentAuthenticatedUser = UserHelpers.GetUserId(User);
            var result = await _chatService.FilterMessagesAsync(filter, currentAuthenticatedUser);
            return StatusCode(result.StatusCode, result);
        }
    }
}
