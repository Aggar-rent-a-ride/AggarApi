﻿using DATA.Models;
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

           builder.HasQueryFilter(v => v.IsDeleted == false);

            builder.ToTable("Vehicles");
        }
    }
}
