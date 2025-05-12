using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addedVehiclePopularityIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VehiclePopularity",
                table: "VehiclePopularity");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "VehiclePopularity",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehiclePopularity",
                table: "VehiclePopularity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePopularity_VehicleId",
                table: "VehiclePopularity",
                column: "VehicleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VehiclePopularity",
                table: "VehiclePopularity");

            migrationBuilder.DropIndex(
                name: "IX_VehiclePopularity_VehicleId",
                table: "VehiclePopularity");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "VehiclePopularity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehiclePopularity",
                table: "VehiclePopularity",
                column: "VehicleId");
        }
    }
}
