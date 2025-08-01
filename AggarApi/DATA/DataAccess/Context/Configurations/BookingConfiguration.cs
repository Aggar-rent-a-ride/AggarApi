﻿using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DATA.DataAccess.Context.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Status)
                .HasConversion(
                s => s.ToString(),
                s => (BookingStatus)Enum.Parse(typeof(BookingStatus), s)
                );

            builder.HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(true);

            builder.HasOne(b => b.Rental)
                .WithOne(r => r.Booking)
                .HasForeignKey<Rental>(r => r.BookingId)
                .IsRequired(false);

            builder.ToTable("Bookings");
        }
    }
}
