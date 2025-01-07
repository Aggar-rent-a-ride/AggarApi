using Microsoft.EntityFrameworkCore.Migrations;
using System.Data;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] {"Name", "NormalizedName"},
                values: new object[,]
                {
                    {"Admin", "ADMIN"},
                    {"User", "USER"},
                    {"Customer", "CUSTOMER"},
                    {"Renter", "RENTER"}
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [AspNetRoles]");
        }
    }
}
