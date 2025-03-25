using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedTypeBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "VehicleBrands",
                columns: new[] { "Name", "Country"},
                values: new object[,]
                {
                    { "Other", "None" }
                });

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Name" },
                values: new object[,]
                {
                    { "Other" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
