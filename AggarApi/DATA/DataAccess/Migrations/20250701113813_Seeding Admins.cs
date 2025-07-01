using DATA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.Migrations
{
    /// <inheritdoc />
    public partial class SeedingAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var hasher = new PasswordHasher<AppUser>();
            var admin = new AppUser
            {
                Id = 1,
                UserName = "Naru",
                NormalizedUserName = "NARU",
                Email = "omarnaru2002@gmail.com",
                NormalizedEmail = "omarnaru2002@gmail.com".ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                AggreedTheTerms = true,
                Address = "Hihya, Sharqiah, Egypet",
                Status = Models.Enums.UserStatus.Active,
                CreatedAt = DateTime.Now,
                Location = new Location { Latitude = 0.0, Longitude = 0.0 },
                Name = "Naru",
                DateOfBirth = new DateOnly(2002, 10, 11),
                WarningCount = 0,
                TotalWarningsCount = 0
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Naru#2002");

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[]
                {
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled",
            "AccessFailedCount", "IsDeleted",
                    "AggreedTheTerms", "Address", "Status", "CreatedAt", "Location_Latitude", "Location_Longitude", "Name", "DateOfBirth", "WarningCount", "TotalWarningsCount"
                },
                values: new object[]
                {
            admin.Id, admin.UserName, admin.NormalizedUserName, admin.Email, admin.NormalizedEmail,
            admin.EmailConfirmed, admin.PasswordHash, admin.SecurityStamp, admin.ConcurrencyStamp,
            false, false, false, 0, false, true, admin.Address, admin.Status.ToString(), admin.CreatedAt, admin.Location.Latitude, admin.Location.Longitude, admin.Name, admin.DateOfBirth.ToString(), admin.WarningCount, admin.TotalWarningsCount,
                }
            );

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { admin.Id, 2 }
            );

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id" },
                values: new object[] { admin.Id }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
