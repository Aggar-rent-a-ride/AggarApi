﻿using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class RecommendedBrandConfiguration : IEntityTypeConfiguration<RecommendedBrand>
    {
        public void Configure(EntityTypeBuilder<RecommendedBrand> builder)
        {
            builder.HasKey(r => new { r.BrandId, r.CustomerId });

            builder.ToTable("RecommendedBrands");
        }
    }
}
