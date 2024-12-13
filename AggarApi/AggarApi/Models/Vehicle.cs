using AggarApi.Models.Contract;
using NetTopologySuite.Geometries;

namespace AggarApi.Models
{
    public class Vehicle : ISoftDeleteable
    {
        public int Id { get; set; }
        public int RenterId { get; set; }
        public DateTime AddedAt { get; set; }
        public int NumOfPassengers { get; set; }
        public double Rate { get; set; } = 0;
        public int Year { get; set; }
        public string? Model { get; set; }
        public string Color { get; set; } = null!;
        public string MainImagePath { get; set; } = null!;
        public Enums.VehicleStatus Status { get; set; }
        public Enums.VehiclePhysicalStatus PhysicalStatus { get; set; }
        public double PricePerHour { get; set; }
        public double PricePerDay { get; set; }
        public double PricePerMonth { get; set; }
        public string? Requirements { get; set; }
        public string? ExtraDetails { get; set; }
        public Point Location { get; set; } = null!;
        public int WarningCount { get; set; } = 0;
        public int? VehicleTypeId { get; set; }
        public int? VehicleBrandId { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }

        public Renter Renter { get; set; } = null!;
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<VehicleImage>? VehicleImages { get; set; }
        public VehicleBrand? VehicleBrand { get; set; }
        public VehicleType? VehicleType { get; set; }
        public ICollection<Customer>? FavoriteCustomers { get; set; }
        public ICollection<Report>? Reports { get; set; }
    }
}
