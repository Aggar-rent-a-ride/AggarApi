using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    [Owned]
    public class StripeAccount
    {
        public string StripeAccountId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

    }
}
