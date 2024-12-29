using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class AdminActionConfiguration : IEntityTypeConfiguration<AdminAction>
    {
        public void Configure(EntityTypeBuilder<AdminAction> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Notification)
                .WithOne(n => n.TargetAdminAction)
                .HasForeignKey<Notification>(n => n.TargetId)
                .IsRequired(false);

            builder.Property(a => a.Type)
                .HasConversion(
                t => t.ToString(),
                t => (AdminActionType)Enum.Parse(typeof(AdminActionType), t)
                );

            builder.ToTable("AdminActions");
        }
    }
}
