using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    public class CustomerReview : Review
    {
        [Precision(2, 1)]
        public double Truthfulness { get; set; }
        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
