﻿using DATA.Models;
using DATA.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATA.DataAccess.Context.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired(true);

            builder.Property(u => u.UserName)
                .IsRequired(true);

            builder.HasIndex(u => u.Email)
                .IsUnique(true);

            builder.HasIndex(u => u.UserName)
                .IsUnique(true);

            builder.Property(u => u.DateOfBirth)
                .HasColumnType("date")
                .IsRequired(true);

            builder.Property(u => u.Status)
                .HasConversion(
                s => s.ToString(),
                s => (UserStatus)Enum.Parse(typeof(UserStatus), s)
                );

            //builder.OwnsOne(u => u.Address);

            builder.OwnsOne(u => u.Location);

            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.Reciver)
                .HasForeignKey(n => n.ReceiverId)
                .IsRequired(true);

            builder.HasMany(u => u.Messages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(true);

            builder.HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.Receiver)
                .HasForeignKey(m => m.ReceiverId)
                .IsRequired(true);

            builder.HasMany(u => u.Reports)
                .WithOne(r => r.Reporter)
                .HasForeignKey(r => r.ReporterId)
                .IsRequired(true);

            builder.OwnsMany(u => u.RefreshTokens)
                .HasIndex(r => r.Token)
                .IsUnique(true);

           builder.HasQueryFilter(u => u.IsDeleted == false);

            builder.UseTptMappingStrategy();
            builder.ToTable("AppUsers");
        }
    }
}
