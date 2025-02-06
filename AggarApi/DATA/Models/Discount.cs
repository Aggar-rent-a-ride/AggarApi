using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    [Owned]
    public class Discount
    {
        public int DaysRequired { get; set; }
        public decimal DiscountPercentage { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
