using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Status)
                .HasConversion(
                s => s.ToString(),
                s => (ReportStatus)Enum.Parse(typeof(ReportStatus), s)
                );

            builder.Property(r => r.TargetType)
                .HasConversion(
                s => s.ToString(),
                s => (TargetType)Enum.Parse(typeof(TargetType), s)
                );

            builder.HasOne(r => r.TargetAppUser)
                .WithMany(a => a.TargetReports)
                .HasForeignKey(r => r.TargetAppUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.TargetVehicle)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.TargetVehicleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.TargetRenterReview)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.TargetRenterReviewId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.TargetCustomerReview)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.TargetCustomerReviewId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.TargetMessage)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.TargetMessageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.ToTable("Reports");
        }
    }
}
