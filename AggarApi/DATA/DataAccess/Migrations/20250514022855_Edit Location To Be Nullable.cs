using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EditLocationToBeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                                name: "Location_Latitude",
                                table: "AppUsers",
                                type: "float",
                                nullable: true,
                                oldClrType: typeof(double),
                                oldType: "float",
                                oldNullable: false);

            migrationBuilder.AlterColumn<double>(
                name: "Location_Longitude",
                table: "AppUsers",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Location_Latitude",
                table: "AppUsers",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Location_Longitude",
                table: "AppUsers",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}
