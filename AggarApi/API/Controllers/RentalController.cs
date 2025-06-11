using CORE.Constants;
using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetRentalHistoryAsync([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            var userId = UserHelpers.GetUserId(User);
            var result = await _rentalService.GetUserRentalHistoryAsync(userId, pageNo, pageSize);
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("refund")]
        [Authorize(Roles = Roles.Customer)]
        public async Task<IActionResult> RefundRentalAsync(int id)
        {
            int customerId = UserHelpers.GetUserId(User);
            var result = await _rentalService.RefundRentalAsync(customerId, id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRentalByIdAsync([FromQuery] int id)
        {
            var result = await _rentalService.GetRentalByIdAsync(id);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = Roles.Customer)]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmRentalAsync([FromQuery] int rentalId, [FromBody] string qrCodeToken)
        {
            int customerId = UserHelpers.GetUserId(User);
            var result = await _rentalService.ConfirmRentalAsync(customerId, rentalId, qrCodeToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
