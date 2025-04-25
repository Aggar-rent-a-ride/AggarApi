using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE VehicleTypes SET SlogenPath = 'Images/VehicleTypes/car.svg' WHERE Name = 'Car'");
            migrationBuilder.Sql("UPDATE VehicleTypes SET SlogenPath = 'Images/VehicleTypes/bus.svg' WHERE Name = 'Bus'");
            migrationBuilder.Sql("UPDATE VehicleTypes SET SlogenPath = 'Images/VehicleTypes/bicycle.svg' WHERE Name = 'Bicycle'");
            migrationBuilder.Sql("UPDATE VehicleTypes SET SlogenPath = 'Images/VehicleTypes/motorcycle.svg' WHERE Name = 'Motorcycle'");
            migrationBuilder.Sql("UPDATE VehicleTypes SET SlogenPath = 'Images/VehicleTypes/truck.svg' WHERE Name = 'Truck'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
