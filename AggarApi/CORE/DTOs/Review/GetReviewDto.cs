using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Review
{
    public class GetReviewDto
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime CreatedAt { get; set; }
        private double _behavior;
        public double Behavior { get => Math.Round(_behavior, 1); set => _behavior = value; }
        private double _punctuality;
        public double Punctuality { get => Math.Round(_punctuality, 1); set => _punctuality = value; }
        public string? Comments { get; set; }
        private double? _care;
        public double? Care { get => _care.HasValue ? Math.Round(_care.Value, 1) : null; set => _care = value; } // if renter review
        private double? _truthfulness;
        public double? Truthfulness { get => _truthfulness.HasValue ? Math.Round(_truthfulness.Value, 1) : null; set => _truthfulness = value; } // if customer review
    }
}
