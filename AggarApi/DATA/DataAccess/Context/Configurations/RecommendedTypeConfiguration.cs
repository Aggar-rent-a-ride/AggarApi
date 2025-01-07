using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class RecommendedTypeConfiguration : IEntityTypeConfiguration<RecommendedType>
    {
        public void Configure(EntityTypeBuilder<RecommendedType> builder)
        {
            builder.HasKey(r => new { r.TypeId, r.CustomerId });

            builder.ToTable("RecommendedTypes");
        }
    }
}
