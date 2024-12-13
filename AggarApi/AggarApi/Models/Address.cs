using Microsoft.EntityFrameworkCore;

namespace AggarApi.Models
{
    [Owned]
    public class Address
    {
        public string Country { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
    }
}
