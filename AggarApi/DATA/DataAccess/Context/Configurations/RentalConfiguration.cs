using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Status)
               .HasConversion(
               s => s.ToString(),
               s => (RentalStatus)Enum.Parse(typeof(RentalStatus), s)
               );


            builder.ToTable("Rentals");
        }
    }
}
