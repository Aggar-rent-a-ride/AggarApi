using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Discount
{
    public class CreateDiscountDto
    {
        public int DaysRequired { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
