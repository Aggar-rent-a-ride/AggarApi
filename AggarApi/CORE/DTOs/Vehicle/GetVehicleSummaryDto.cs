using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class GetVehicleSummaryDto
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public string? Brand { get; set; }
        public string? Type { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public double PricePerHour { get; set; }
        public bool IsFavourite { get; set; }
        public string Transmission { get; set; } = null!;
        public double? Rate { get; set; }
        public IFormFile MainImage { get; set; } = null!;

    }
}
