using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class RenterVehiclesDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehicleTransmission Transmission { get; set; }
        public double? Rate { get; set; }
        public string? MainImagePath { get; set; }
    }
}
