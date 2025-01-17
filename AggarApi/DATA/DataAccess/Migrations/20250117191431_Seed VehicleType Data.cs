using Microsoft.EntityFrameworkCore.Migrations;
using System.Net.Http.Headers;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedVehicleTypeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Name", "SlogenPath" },
                values: new object[,]
                {
                    { "Car", "Images/VehicleTypes/Car.png" },
                    { "Bicycle", "Images/VehicleTypes/Bicycle.png" },
                    { "Truck", "Images/VehicleTypes/Truck.png" },
                    { "Motorcycle", "Images/VehicleTypes/Motorcycle.png" },
                    { "Bus", "Images/VehicleTypes/Bus.png" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
