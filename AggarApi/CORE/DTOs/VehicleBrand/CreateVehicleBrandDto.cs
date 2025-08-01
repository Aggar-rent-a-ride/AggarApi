﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CORE.DTOs.VehicleBrand
{
    public class CreateVehicleBrandDto
    {
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public IFormFile? Logo { get; set; }
    }
}
