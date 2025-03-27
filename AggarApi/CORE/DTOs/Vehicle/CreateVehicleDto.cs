using CORE.DTOs.Discount;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public ICollection<IFormFile>? Images { get; set; }
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
        public string? Address { get; set; }
        public int VehicleTypeId { get; set; }
        public int VehicleBrandId { get; set; }
    }
}
