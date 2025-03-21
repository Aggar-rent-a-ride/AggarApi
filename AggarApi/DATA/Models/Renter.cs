namespace DATA.Models
{
    public class Renter : AppUser
    {
        public StripeAccount StripeAccount { get; set; } = null!;
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<RenterReview>? Reviews { get; set; }
    }
}
