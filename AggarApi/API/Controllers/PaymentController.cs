using CORE.Constants;
using CORE.DTOs.Payment;
using CORE.Helpers;
using CORE.Services;
using CORE.Services.IServices;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly StripeSettings _stripe;
        private readonly ILogger<PaymentController> _logger;
        private readonly IBookingService _bookingService;
        private readonly IRentalService _rentalService;

        public PaymentController(IPaymentService paymentService, IOptions<StripeSettings> stripeSettings, ILogger<PaymentController> logger, IBookingService bookingService, IRentalService rentalService)
        {
            _paymentService = paymentService;
            _stripe = stripeSettings.Value;
            _logger = logger;
            _bookingService = bookingService;
            _rentalService = rentalService;
        }

        [Authorize(Roles = Roles.Renter)]
        [HttpPost("connected-account")]
        public async Task<IActionResult> CreateConnectedAccount(CreateConnectedAccountDto dto)
        {
            int renterId = UserHelpers.GetUserId(User);
            var response = await _paymentService.CreateStripeAccountAsync(dto, renterId);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string webhookSecret = _stripe.WebhookSecret;

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentFailed(stripeEvent);
                        break;

                    case "charge.refunded":
                        //await HandleChargeRefunded(stripeEvent);
                        break;

                    default:
                        _logger.LogWarning("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Error processing webhook");
                return BadRequest();
            }
        }

        private async Task HandlePaymentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent.Metadata.TryGetValue("BookingId", out string bookingIdStr) &&
                int.TryParse(bookingIdStr, out int bookingId))
            {
                await _bookingService.HandleBookingPaymentSuccededAsync(bookingId, paymentIntent.Id);
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent.Metadata.TryGetValue("BookingId", out string bookingIdStr) &&
                int.TryParse(bookingIdStr, out int bookingId))
            {
                await _bookingService.HandleBookingPaymentFailedAsync(bookingId, paymentIntent.Id);
            }
        }
    }
}
