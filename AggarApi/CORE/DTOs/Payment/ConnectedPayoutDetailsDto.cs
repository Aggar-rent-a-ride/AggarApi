using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class ConnectedPayoutDetailsDto
    {
        public string Last4 { get; set; }
        public string RoutingNumber { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal UpcomingAmount { get; set; }
        public decimal TotalBalance => CurrentAmount + UpcomingAmount;
        public string Currency { get; set; }
    }
}
