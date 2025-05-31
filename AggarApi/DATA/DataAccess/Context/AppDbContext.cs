using DATA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DATA.DataAccess.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Renter> Renters { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<CustomerReview> CustomerReviews { get; set; }
        public DbSet<RenterReview> RenterReviews { get; set; }
        public DbSet<Booking> bookings { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RecommendedBrand> RecommendedBrands { get; set; }
        public DbSet<RecommendedType> RecommendedTypes { get; set; }
        public DbSet<FileCache> FileCache { get; set; }
        public DbSet<VehiclePopularity> VehiclePopularity { get; set; }
        public DbSet<CustomersFavoriteVehicles> CustomersFavoriteVehicles { get; set; }

        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.ApplyConfiguration(new());

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
