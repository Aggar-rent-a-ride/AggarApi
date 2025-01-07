using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.HasMany(a => a.Actions)
                .WithOne(a => a.Admin)
                .HasForeignKey(a => a.AdminId)
                .IsRequired(true);

            builder.ToTable("Admins");
        }
    }
}
