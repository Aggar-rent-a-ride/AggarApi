namespace DATA.Models
{
    public class RenterReview : Review
    {
        public double Care { get; set; }
        public int RenterId { get; set; }

        public Renter Renter { get; set; } = null!;
    }
}
