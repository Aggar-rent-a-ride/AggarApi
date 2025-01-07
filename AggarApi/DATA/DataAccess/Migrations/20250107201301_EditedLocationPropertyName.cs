using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EditedLocationPropertyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location_Langitude",
                table: "Vehicles",
                newName: "Location_Longitude");

            migrationBuilder.RenameColumn(
                name: "Location_Langitude",
                table: "AppUsers",
                newName: "Location_Longitude");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location_Longitude",
                table: "Vehicles",
                newName: "Location_Langitude");

            migrationBuilder.RenameColumn(
                name: "Location_Longitude",
                table: "AppUsers",
                newName: "Location_Langitude");
        }
    }
}
