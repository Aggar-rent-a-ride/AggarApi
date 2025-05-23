using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Booking
{
    public class ConfirmBookingDto
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId {  get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
