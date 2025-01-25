using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    public class VehicleImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = null!;
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
