using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamedSeenInMessageToIsSeen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Seen",
                table: "Messages",
                newName: "IsSeen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSeen",
                table: "Messages",
                newName: "Seen");
        }
    }
}
