using DATA.Models.Contract;

namespace DATA.Models
{
    public class VehicleBrand : ISoftDeleteable
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? LogoPath { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }
    }
}
