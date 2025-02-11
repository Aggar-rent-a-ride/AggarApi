using DATA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Context.Interceptors
{
    public class UserConnectionConfiguration : IEntityTypeConfiguration<UserConnection>
    {
        public void Configure(EntityTypeBuilder<UserConnection> builder)
        {
            builder
            .HasOne(uc => uc.User)
            .WithMany(u=>u.Connections) 
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // Delete connections if user is deleted

            builder.HasIndex(uc => uc.ConnectionId);
        }
    }
}
