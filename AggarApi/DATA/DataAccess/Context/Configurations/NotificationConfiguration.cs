using DATA.Models;
using Microsoft.EntityFrameworkCore;

namespace DATA.DataAccess.Context.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.ToTable("Notifications");
        }
    }
}
