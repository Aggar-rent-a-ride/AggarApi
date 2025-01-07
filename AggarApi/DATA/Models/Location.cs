using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    [Owned]
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
