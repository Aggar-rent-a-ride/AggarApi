using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class Payment
    {
        [Key]
        public string PaymentIntentId { get; set; } = null!;
        public int BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime CapturedAt { get; set; }
        public ICollection<Refund>? Refunds { get; set; }
        public ICollection<Transfers>? Transfers { get; set; }
    }
}
