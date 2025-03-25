using DATA.Models.Contract;

namespace DATA.Models
{
    public class VehicleType : ISoftDeleteable
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? SlogenPath { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }
    }
}
