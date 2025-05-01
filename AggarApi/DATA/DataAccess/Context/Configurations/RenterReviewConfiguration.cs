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

            builder.ToTable("RenterReviews");
        }
    }
}
