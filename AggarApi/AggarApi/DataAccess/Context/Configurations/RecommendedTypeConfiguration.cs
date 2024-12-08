using AggarApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AggarApi.DataAccess.Context.Configurations
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
