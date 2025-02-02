using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public int DaysRequired { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountedPricePerDay { get; set; }
    }
}
