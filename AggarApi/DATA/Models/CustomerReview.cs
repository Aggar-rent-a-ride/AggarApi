using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    public class CustomerReview : Review
    {
        [Precision(1, 1)]
        public double Truthfulness { get; set; }
        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
