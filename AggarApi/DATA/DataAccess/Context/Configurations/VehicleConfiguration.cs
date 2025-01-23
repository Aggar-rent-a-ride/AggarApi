using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.HasKey(v => v.Id);

            builder.OwnsOne(v => v.Address);

            builder.OwnsOne(v => v.Location);

            builder.OwnsMany(v => v.VehicleImages);

            builder.HasOne(v => v.VehicleBrand)
                .WithMany(v => v.Vehicles)
                .HasForeignKey(v => v.VehicleBrandId)
                .IsRequired(false);

            builder.HasOne(v => v.VehicleType)
                .WithMany(v => v.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .IsRequired(false);

            builder.Property(v => v.Status)
                .HasConversion(
                s => s.ToString(),
                s => (VehicleStatus)Enum.Parse(typeof(VehicleStatus), s)
                );

            builder.Property(v => v.PhysicalStatus)
                .HasConversion(
                s => s.ToString(),
                s => (VehiclePhysicalStatus)Enum.Parse(typeof(VehiclePhysicalStatus), s)
                );

            builder.Property(v => v.Transmission)
                .HasConversion(
                s => s.ToString(),
                s => (VehicleTransmission)Enum.Parse(typeof(VehicleTransmission), s)
                );

            builder.HasMany(v => v.Reports)
                .WithOne(r => r.TargetVehicle)
                .HasForeignKey(r => r.TargetId)
                .IsRequired(false);

            builder.ToTable("Vehicles");
        }
    }
}
