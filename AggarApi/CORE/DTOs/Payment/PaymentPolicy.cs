using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class PaymentPolicy
    {
        public int FeesPercentage { get; set; }
        public int AllowedConfirmDays { get; set; }
        public int AllowedRefundDaysBefore { get; set; }
        public int RefundPenalityPercentage { get; set; }
    }
}
