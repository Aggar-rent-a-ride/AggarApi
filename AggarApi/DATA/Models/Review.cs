using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Precision(2, 1)]
        public double Behavior { get; set; }
        [Precision(2, 1)]
        public double Punctuality { get; set; }
        public string? Comments { get; set; }

        public Rental Rental { get; set; } = null!;
    }
}
