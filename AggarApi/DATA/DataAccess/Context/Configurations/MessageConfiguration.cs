using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
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

            builder.HasDiscriminator(m=>m.MessageType)
                .HasValue<ContentMessage>("Content")
                .HasValue<FileMessage>("File");

            builder.Property(m => m.MessageType).HasMaxLength(50);
            builder.HasIndex(m => m.MessageType);
            builder.HasIndex(m => m.SentAt);

            builder.ToTable("Messages");
        }
    }
}
