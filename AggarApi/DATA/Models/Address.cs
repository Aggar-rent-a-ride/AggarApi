using Microsoft.EntityFrameworkCore;

namespace DATA.Models
{
    [Owned]
    public class Address
    {
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
    }
}
