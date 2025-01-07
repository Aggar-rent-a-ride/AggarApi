using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class CustomerReviewConfiguration : IEntityTypeConfiguration<CustomerReview>
    {
        public void Configure(EntityTypeBuilder<CustomerReview> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Rental)
                .WithOne(r => r.CustomerReview)
                .HasForeignKey<CustomerReview>(r => r.RentalId)
                .IsRequired(true);

            builder.HasOne(r => r.Notification)
                .WithOne(n => n.TargetCustomerReview)
                .HasForeignKey<Notification>(n => n.TargetId)
                .IsRequired(false);

            builder.HasMany(r => r.Reports)
                .WithOne(r => r.TargetCustomerReview)
                .HasForeignKey(r => r.TargetId)
                .IsRequired(false);

            builder.ToTable("CustomerReviews");
        }
    }
}
