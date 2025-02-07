using CORE.DTOs.Booking;
using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService? bookingService)
        {
            _bookingService = bookingService;
        }

        [Authorize(Roles = "Custoemr")]
        [HttpPost]
        public async Task<IActionResult> CreateBookingAsync([FromForm] CreateBookingDto createBookingDto)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response =  await _bookingService.CreateBookingAsync(createBookingDto, customerId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingAsync(int id)
        {
            int userId = UserHelpers.GetUserId(User);
            var response = await _bookingService.GetBookingByIdAsync(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("cancel-booking/{id}")]
        public async Task<IActionResult> CancelBookingAsync(int id)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.CancelBookingAsync(id, customerId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Renter")]
        [HttpPut("response-booking/{id}")]
        public async Task<IActionResult> ResponseBookingAsync(int id, [FromQuery] bool isAccepted)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.ResponseBookingRequestAsync(id, customerId, isAccepted);
            return StatusCode(response.StatusCode, response);
        }
    }
}
