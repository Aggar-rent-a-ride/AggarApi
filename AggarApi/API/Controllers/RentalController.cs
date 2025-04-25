using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    }
}
