using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class StripeAccountCreation
    {
        public string StripeAccountId { get; set; }
        public string BankAccountId { get; set; }
        public bool IsVerified { get; set; }
    }
}
