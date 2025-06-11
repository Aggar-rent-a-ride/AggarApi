using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class PlatformBalanceDto
    {
        public decimal AvailableBalanc { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal ConnectReserved { get; set; }
        public decimal TotalBalance => AvailableBalanc + PendingBalance;
        public string Currency { get; set; }
    } 
}
