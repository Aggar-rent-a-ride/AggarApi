using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class RenterConfiguration : IEntityTypeConfiguration<Renter>
    {
        public void Configure(EntityTypeBuilder<Renter> builder)
        {
            builder.HasMany(c => c.Vehicles)
                .WithOne(v => v.Renter)
                .HasForeignKey(v => v.RenterId)
                .IsRequired(true);

            builder.HasMany(r => r.Reviews)
                .WithOne(r => r.Renter)
                .HasForeignKey(r => r.RenterId)
                .IsRequired(true);

            builder.ToTable("Renters");
        }
    }
}
