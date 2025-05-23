using DATA.Models.Contract;

namespace DATA.Models
{
    public class Vehicle : ISoftDeleteable
    {
        public int Id { get; set; }
        public int RenterId { get; set; }
        public DateTime AddedAt { get; set; }
        public int NumOfPassengers { get; set; }
        public double? Rate { get; set; }
        public int Year { get; set; }
        public string? Model { get; set; }
        public string Color { get; set; } = null!;
        public string MainImagePath { get; set; } = null!;
        public Enums.VehicleStatus Status { get; set; }
        public Enums.VehiclePhysicalStatus PhysicalStatus { get; set; }
        public Enums.VehicleTransmission Transmission { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Requirements { get; set; }
        public string? ExtraDetails { get; set; }
        public string? Address { get; set; }
        public Location? Location { get; set; }
        public int VehicleTypeId { get; set; }
        public int VehicleBrandId { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }
        public Renter Renter { get; set; } = null!;
        public VehiclePopularity VehiclePopularity { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<VehicleImage>? VehicleImages { get; set; }
        public VehicleBrand VehicleBrand { get; set; } = null!;
        public VehicleType VehicleType { get; set; } = null!;
        public ICollection<Customer>? FavoriteCustomers { get; set; }
        public ICollection<Report>? Reports { get; set; }
        public ICollection<Discount>? Discounts { get; set; }
    }
}
