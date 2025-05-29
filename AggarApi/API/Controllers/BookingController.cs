using CORE.Constants;
using CORE.DTOs.Booking;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Models.Enums;
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

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateBookingAsync([FromForm] CreateBookingDto createBookingDto)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response =  await _bookingService.CreateBookingAsync(createBookingDto, customerId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBookingAsync([FromQuery] int id)
        {
            int userId = UserHelpers.GetUserId(User);
            var response = await _bookingService.GetBookingDetailsByIdAsync(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("cancel")]
        public async Task<IActionResult> CancelBookingAsync([FromQuery] int id)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.CancelBookingAsync(id, customerId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Renter")]
        [HttpPut("response")]
        public async Task<IActionResult> ResponseBookingAsync([FromQuery] int id, [FromQuery] bool isAccepted)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.ResponseBookingRequestAsync(id, customerId, isAccepted);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("bookings-by-status")]
        public async Task<IActionResult> GetBookingsByStatusAsync([FromQuery] BookingStatus? status, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _bookingService.GetBookingsByStatusAsync(status, pageNo, pageSize);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("count-bookings-by-status")]
        public async Task<IActionResult> GetBookingsByStatusCountAsync([FromQuery] BookingStatus? status)
        {
            var response = await _bookingService.GetBookingsByStatusCountAsync(status);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = Roles.Customer)]
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmBookingAsnc([FromQuery]int id)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.ConfirmBookingAsync(customerId, id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
