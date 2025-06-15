using DATA.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.AppUser
{
    public class UpdateProfileDto
    {
        public string Name { get; set; }
        public IFormFile? Image { get; set; }
        public Location Location { get; set; }
        public string? Address { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Bio { get; set; }
    }
}
