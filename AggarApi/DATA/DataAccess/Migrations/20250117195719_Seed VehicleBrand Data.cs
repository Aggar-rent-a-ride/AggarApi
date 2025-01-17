using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedVehicleBrandData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "VehicleBrands",
                columns: new[] { "Name", "Country", "LogoPath" },
                values: new object[,]
                {
                    { "Toyota", "Japan", "Images/VehicleBrands/Toyota.png" },
                    { "Hyundai", "South Korean", "Images/VehicleBrands/Hyundai.png" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
