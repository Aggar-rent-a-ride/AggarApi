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

            builder.ToTable("CustomerReviews");
        }
    }
}
