using Microsoft.EntityFrameworkCore;

namespace AggarApi.Models
{
    [Owned]
    public class VehicleImage
    {
        public string ImagePath { get; set; } = null!;
    }
}
