using CORE.DTOs.Payment;
using CORE.Helpers;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        [HttpPost("connected-account")]
        public async Task<IActionResult> CreateConnectedAccount(CreateConnectedAccountDto dto)
        {
            int renterId = UserHelpers.GetUserId(User);
            var response = await _paymentService.CreateStripeAccountAsync(dto, renterId);

            return StatusCode(response.StatusCode, response);
        }
    }
}
