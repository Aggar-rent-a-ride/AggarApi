using AggarApi.Models;
using AggarApi.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AggarApi.DataAccess.Context.Configurations
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

            builder.HasOne(b => b.Notification)
                .WithOne(n => n.TargetBooking)
                .HasForeignKey<Notification>(n => n.TargetId)
                .IsRequired(false);

            builder.ToTable("Bookings");
        }
    }
}
