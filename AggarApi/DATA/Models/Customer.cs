namespace DATA.Models
{
    public class Customer : AppUser
    {
        public ICollection<RecommendedBrand>? RecommendedBrands { get; set; }
        public ICollection<RecommendedType>? RecommendedTypes { get; set; }
        public ICollection<Vehicle>? FavoriteVehicles { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<CustomerReview>? Reviews { get; set; }

    }
}
