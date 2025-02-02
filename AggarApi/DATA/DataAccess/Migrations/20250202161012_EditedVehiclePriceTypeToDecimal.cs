using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EditedVehiclePriceTypeToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerDay",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "PricePerDay",
                table: "Vehicles",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
