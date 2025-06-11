using CORE.DTOs;
using CORE.DTOs.Payment;
using DATA.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IPaymentService
    {
        public Task<ResponseDto<StripeAccountDto>> CreateStripeAccountAsync(CreateConnectedAccountDto dto, int renterId);
        public Task<PaymentIntent?> CreatePaymentIntent(Booking booking);
        public Task<Transfer?> TransferToRenterAsync(string paymentIntentId, string connectedAccountId, int rentalId, long amount);
        Task<Refund?> RefundAsync(string paymentIntentId, string connectedAccountId, int rentalId, long amount);
        Task<ResponseDto<PlatformBalanceDto>> PlatformBalanceAsync();
        Task<ResponseDto<ConnectedPayoutDetailsDto>> GetRenterPayoutDetailsAsync(int renterID);
    }
}
