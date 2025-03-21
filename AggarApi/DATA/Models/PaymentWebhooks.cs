using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class PaymentWebhooks
    {
        public string Id { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime ReceivedAt { get; set; }
    }
}
