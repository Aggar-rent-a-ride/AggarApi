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
                t => t.ToString(),
                t => (TargetType)Enum.Parse(typeof(TargetType), t)
                );

            builder.ToTable("Reports");
        }
    }
}
