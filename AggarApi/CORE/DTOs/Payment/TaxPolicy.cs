using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class TaxPolicy
    {
        public int FeesPercentage { get; set; }
        public int RefundPercentage { get; set; }
    }
}
