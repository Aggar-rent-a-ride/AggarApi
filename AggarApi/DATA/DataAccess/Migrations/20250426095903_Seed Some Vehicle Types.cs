using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedSomeVehicleTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE VehicleTypes SET SlogenPath = 'Images/VehicleTypes/motorcycles.svg' WHERE Name = 'Motorcycle'");

            migrationBuilder.InsertData(
                table: "VehicleTypes",
                columns: new[] { "Name", "SlogenPath" },
                values: new object[,]
                {
                    { "Recreational", "Images/VehicleTypes/recreational.svg" },
                    { "Taxi", "Images/VehicleTypes/taxi.svg" },
                    { "Van", "Images/VehicleTypes/van.svg" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
