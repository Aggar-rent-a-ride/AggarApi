namespace DATA.Models
{
    public class VehicleType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? SlogenPath { get; set; }

        public ICollection<Vehicle>? Vehicles { get; set; }
    }
}
