using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addedTotalWarningsCountToAppUserAndRenamedActivateInColumnToBannedTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActivateIn",
                table: "AppUsers",
                newName: "BannedTo");

            migrationBuilder.AddColumn<int>(
                name: "TotalWarningsCount",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalWarningsCount",
                table: "AppUsers");

            migrationBuilder.RenameColumn(
                name: "BannedTo",
                table: "AppUsers",
                newName: "ActivateIn");
        }
    }
}
