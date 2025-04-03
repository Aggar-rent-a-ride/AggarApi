using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Review
{
    public class CreateReviewDto
    {
        public int RentalId { get; set; }
        public double Behavior { get; set; }
        public double Punctuality { get; set; }
        public string? Comments { get; set; }
        public double? Care { get; set; } // if renter review
        public double? Truthfulness { get; set; } // if customer review
    }
}
