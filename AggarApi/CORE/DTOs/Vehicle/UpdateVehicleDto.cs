using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA.Models.Enums;
using System.Text.Json.Serialization;
using DATA.Models;
using CORE.DTOs.Discount;

namespace CORE.DTOs.Vehicle
{
    public class UpdateVehicleDto
    {
        public int Id { get; set; }
        public int NumOfPassengers { get; set; }
        public int Year { get; set; }
        public string? Model { get; set; }
        public string Color { get; set; } = null!;
        public IFormFile? MainImage { get; set; } // if null, don't update it
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehicleStatus Status { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehiclePhysicalStatus PhysicalStatus { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VehicleTransmission Transmission { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Requirements { get; set; }
        public string? ExtraDetails { get; set; }
        public Location Location { get; set; } = null!;
        public Address? Address { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? VehicleBrandId { get; set; }
    }
}
