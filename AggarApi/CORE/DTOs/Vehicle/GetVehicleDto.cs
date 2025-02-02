using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DATA.Models;
using DATA.Models.Enums;

namespace CORE.DTOs.Vehicle
{
    public class GetVehicleDto
    {
        public int Id { get; set; }
        public GetVehicleDtoRenterDto Renter { get; set; }
        public int NumOfPassengers { get; set; }
        public double? Rate { get; set; }
        public int Year { get; set; }
        public string? Model { get; set; }
        public string Color { get; set; } = null!;
        public string MainImagePath { get; set; } = null!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehicleStatus Status { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehiclePhysicalStatus PhysicalStatus { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehicleTransmission Transmission { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Requirements { get; set; }
        public string? ExtraDetails { get; set; }
        public Address? Address { get; set; }
        public VehicleTypeDto VehicleType { get; set; }
        public VehicleBrandDto VehicleBrand { get; set; }
        public ICollection<string>? VehicleImages { get; set; }

    }
}
