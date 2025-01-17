using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    [Owned]
    public class VehicleImage
    {
        public string? ImagePath { get; set; }
    }
}
