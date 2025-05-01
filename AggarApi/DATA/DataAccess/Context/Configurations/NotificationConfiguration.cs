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
                .WithOne(r => r.Notification)
                .HasForeignKey<Notification>(n => n.TargetCustomerReviewId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetRenterReview)
                .WithOne(r => r.Notification)
                .HasForeignKey<Notification>(n => n.TargetRenterReviewId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetBooking)
                .WithOne(b => b.Notification)
                .HasForeignKey<Notification>(n => n.TargetBookingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetMessage)
                .WithOne(m => m.Notification)
                .HasForeignKey<Notification>(n => n.TargetMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.TargetRental)
                .WithOne(r => r.Notification)
                .HasForeignKey<Notification>(n => n.TargetRentalId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.ToTable("Notifications");
        }
    }
}
