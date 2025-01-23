using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class VehicleTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? SlogenPath { get; set; }
    }
}
