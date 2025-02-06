using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EditDiscountEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Discount",
                table: "Discount");

            migrationBuilder.DropIndex(
                name: "IX_Discount_VehicleId",
                table: "Discount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Discount",
                table: "Discount",
                columns: new[] { "VehicleId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Discount",
                table: "Discount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Discount",
                table: "Discount",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Discount_VehicleId",
                table: "Discount",
                column: "VehicleId");
        }
    }
}
