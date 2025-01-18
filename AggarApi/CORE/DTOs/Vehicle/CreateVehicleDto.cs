using DATA.Models;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class CreateVehicleDto
    {
        public int NumOfPassengers { get; set; }
        public int Year { get; set; }
        public string? Model { get; set; }
        public string Color { get; set; } = null!;
        public IFormFile MainImage { get; set; }
        public VehicleStatus Status { get; set; }
        public VehiclePhysicalStatus PhysicalStatus { get; set; }
        public double PricePerHour { get; set; }
        public double PricePerDay { get; set; }
        public double PricePerMonth { get; set; }
        public string? Requirements { get; set; }
        public string? ExtraDetails { get; set; }
        public Location Location { get; set; } = null!;
        public int? VehicleTypeId { get; set; }
        public int? VehicleBrandId { get; set; }
    }
}
