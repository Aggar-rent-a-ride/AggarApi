using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.AppUser
{
    public class SummerizedUserWithRateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string? ImagePath { get; set; }
        public double? Rate { get; set; }
    }
}
