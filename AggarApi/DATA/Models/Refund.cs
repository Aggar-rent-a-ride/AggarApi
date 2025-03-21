using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class Refund
    {
        [Key]
        public string RefundId { get; set; } = null!;
        public int PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public Transfers Transfer { get; set; } = null!;
    }
}
