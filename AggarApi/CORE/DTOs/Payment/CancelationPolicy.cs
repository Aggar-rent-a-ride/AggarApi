using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class CancelationPolicy
    {
        public int DaysAfterBooking { get; set; }
        public int PenaltyPercentage { get; set; }
    }
}
