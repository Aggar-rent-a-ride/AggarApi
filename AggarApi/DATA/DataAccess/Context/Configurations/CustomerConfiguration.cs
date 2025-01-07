using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasMany(c => c.RecommendedBrands)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId)
                .IsRequired(true);

            builder.HasMany(c => c.RecommendedTypes)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId)
                .IsRequired(true);

            builder.HasMany(c => c.FavoriteVehicles)
                .WithMany(v => v.FavoriteCustomers)
                .UsingEntity<Dictionary<string, object>>(
                "CustomersFavoriteVehicles",
                right => right
                .HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey("VehicleId")
                .OnDelete(DeleteBehavior.NoAction),
                left => left
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade));

            builder.HasMany(c => c.Bookings)
                .WithOne(b => b.Customer)
                .HasForeignKey(b => b.CustomerId)
                .IsRequired(true);

            builder.HasMany(c => c.Reviews)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId)
                .IsRequired(true);

            builder.ToTable("Customers");
        }
    }
}
