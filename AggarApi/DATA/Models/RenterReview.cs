using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    public class RenterReview : Review
    {
        [Precision(2, 1)]
        public double Care { get; set; } // vhiecle status after the rental
        public int RenterId { get; set; }

        public Renter Renter { get; set; } = null!;
        public ICollection<Report>? Reports { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
}
