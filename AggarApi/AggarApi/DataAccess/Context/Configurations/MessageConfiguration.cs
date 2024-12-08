using AggarApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AggarApi.DataAccess.Context.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasOne(m => m.Notification)
                .WithOne(n => n.TargetMessage)
                .HasForeignKey<Notification>(n => n.TargetId)
                .IsRequired(false);

            builder.HasOne(m => m.Report)
                .WithOne(r => r.TargetMessage)
                .HasForeignKey<Report>(r => r.TargetId)
                .IsRequired(false);

            builder.ToTable("Messages");
        }
    }
}
