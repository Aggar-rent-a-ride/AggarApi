using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DATA.DataAccess.Context.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.TargetType)
                .HasConversion(
                s => s.ToString(),
                s => (TargetType)Enum.Parse(typeof(TargetType), s)
                );

            builder.HasOne(n => n.TargetCustomerReview)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.TargetCustomerReviewId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetRenterReview)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.TargetRenterReviewId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetBooking)
                .WithMany(b => b.Notifications)
                .HasForeignKey(n => n.TargetBookingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetMessage)
                .WithMany(m => m.Notifications)
                .HasForeignKey(n => n.TargetMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetRental)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.TargetRentalId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.ToTable("Notifications");
        }
    }
}
