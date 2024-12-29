using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class RenterReviewConfiguration : IEntityTypeConfiguration<RenterReview>
    {
        public void Configure(EntityTypeBuilder<RenterReview> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Rental)
                .WithOne(r => r.RenterReview)
                .HasForeignKey<RenterReview>(r => r.RentalId)
                .IsRequired(true);

            builder.HasOne(r => r.Notification)
                .WithOne(n => n.TargetRenterReview)
                .HasForeignKey<Notification>(n => n.TargetId)
                .IsRequired(false);

            builder.HasMany(r => r.Reports)
                .WithOne(r => r.TargetRenterReview)
                .HasForeignKey(r => r.TargetId)
                .IsRequired(false);

            builder.ToTable("RenterReviews");
        }
    }
}
