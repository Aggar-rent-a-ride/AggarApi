using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    public class RenterReview : Review
    {
        [Precision(1, 1)]
        public double Care { get; set; } // vhiecle status after the rental
        public int RenterId { get; set; }

        public Renter Renter { get; set; } = null!;
    }
}
