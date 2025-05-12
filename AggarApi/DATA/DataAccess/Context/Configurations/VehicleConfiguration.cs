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

            //builder.OwnsOne(v => v.Address);

            builder.OwnsOne(v => v.Location);

            builder.HasMany(v => v.VehicleImages)
                .WithOne(i => i.Vehicle)
                .HasForeignKey(i => i.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.VehicleBrand)
                .WithMany(v => v.Vehicles)
                .HasForeignKey(v => v.VehicleBrandId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);

            builder.HasOne(v => v.VehicleType)
                .WithMany(v => v.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);

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

            builder.OwnsMany(v => v.Discounts)
                .WithOwner(d => d.Vehicle);

            builder.HasMany(v=>v.VehiclePopularity).WithMany(u=>u.VehiclePopularity)
                .UsingEntity<VehiclePopularity>(
                    j => j
                        .HasOne(vp => vp.AppUser)
                        .WithMany()
                        .HasForeignKey(vp => vp.AppUserId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne(vp => vp.Vehicle)
                        .WithMany()
                        .HasForeignKey(vp => vp.VehicleId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(vp => new { vp.VehicleId, vp.AppUserId });
                        j.Property(vp => vp.LastTimeVisited).IsRequired();
                    });

            builder.ToTable("Vehicles");
        }
    }
}
