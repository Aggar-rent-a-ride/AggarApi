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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingAsync(int id)
        {
            int userId = UserHelpers.GetUserId(User);
            var response = await _bookingService.GetBookingDetailsByIdAsync(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("cancel-booking")]
        public async Task<IActionResult> CancelBookingAsync([FromQuery] int bookingId)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.CancelBookingAsync(bookingId, customerId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Renter")]
        [HttpPut("response-booking")]
        public async Task<IActionResult> ResponseBookingAsync([FromQuery] int bookingId, [FromQuery] bool isAccepted)
        {
            int customerId = UserHelpers.GetUserId(User);
            var response = await _bookingService.ResponseBookingRequestAsync(bookingId, customerId, isAccepted);
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
