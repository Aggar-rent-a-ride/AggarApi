using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class Transfers
    {
        [Key]
        public string TransferId { get; set; } = null!;
        public int PaymentId { get; set; }
        public string AccountId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public Payment Payment { get; set; } = null!;
    }
}
