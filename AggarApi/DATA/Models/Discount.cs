using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int DaysRequired { get; set; }
        public double DiscountPercentage { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
