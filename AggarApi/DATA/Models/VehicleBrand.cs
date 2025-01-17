namespace DATA.Models
{
    public class VehicleBrand
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? LogoPath { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }
    }
}
