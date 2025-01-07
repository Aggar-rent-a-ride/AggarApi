namespace DATA.Models
{
    public class Renter : AppUser
    {
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<RenterReview>? Reviews { get; set; }
    }
}
