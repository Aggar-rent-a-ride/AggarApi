using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MaintainenceForReportReviewRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Review_ReviewId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReviewId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "WarningCount",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ReviewId",
                table: "Reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarningCount",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReviewId",
                table: "Reports",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Review_ReviewId",
                table: "Reports",
                column: "ReviewId",
                principalTable: "Review",
                principalColumn: "Id");
        }
    }
}
