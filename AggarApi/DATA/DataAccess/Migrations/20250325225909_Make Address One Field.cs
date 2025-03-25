using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeAddressOneField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppUsers");

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
                name: "Address_State",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
