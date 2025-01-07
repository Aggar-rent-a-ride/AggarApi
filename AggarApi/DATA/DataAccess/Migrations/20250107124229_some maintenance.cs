using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class somemaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AppUsers");

            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "Vehicles",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Governorate",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Langitude",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "AppUsers",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<double>(
                name: "Location_Langitude",
                table: "AppUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "AppUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_Governorate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Location_Langitude",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Location_Langitude",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "AppUsers");

            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Vehicles",
                type: "geography",
                nullable: false);

            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "AppUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "AppUsers",
                type: "geography",
                nullable: false);
        }
    }
}
