using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CORE.DTOs.VehicleType
{
    public class CreateVehicleTypeDto
    {
        public string Name { get; set; } = null!;
        public IFormFile? Slogan { get; set; }
    }
}
