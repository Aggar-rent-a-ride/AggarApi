using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceTargetRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Bookings_TargetId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CustomerReviews_TargetId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Messages_TargetId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_RenterReviews_TargetId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AppUsers_TargetId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_CustomerReviews_TargetId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Messages_TargetId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_RenterReviews_TargetId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Vehicles_TargetId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TargetId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TargetId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "Reports",
                newName: "TargetVehicleId");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "Notifications",
                newName: "TargetRenterReviewId");

            migrationBuilder.RenameColumn(
                name: "Target",
                table: "Notifications",
                newName: "TargetRentalId");

            migrationBuilder.AddColumn<int>(
                name: "TargetAppUserId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetCustomerReviewId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetMessageId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetRenterReviewId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetBookingId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetCustomerReviewId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetMessageId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetType",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TargetAppUserId",
                table: "Reports",
                column: "TargetAppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TargetCustomerReviewId",
                table: "Reports",
                column: "TargetCustomerReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TargetMessageId",
                table: "Reports",
                column: "TargetMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TargetRenterReviewId",
                table: "Reports",
                column: "TargetRenterReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TargetVehicleId",
                table: "Reports",
                column: "TargetVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetBookingId",
                table: "Notifications",
                column: "TargetBookingId",
                unique: true,
                filter: "[TargetBookingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetCustomerReviewId",
                table: "Notifications",
                column: "TargetCustomerReviewId",
                unique: true,
                filter: "[TargetCustomerReviewId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetMessageId",
                table: "Notifications",
                column: "TargetMessageId",
                unique: true,
                filter: "[TargetMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetRentalId",
                table: "Notifications",
                column: "TargetRentalId",
                unique: true,
                filter: "[TargetRentalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetRenterReviewId",
                table: "Notifications",
                column: "TargetRenterReviewId",
                unique: true,
                filter: "[TargetRenterReviewId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Bookings_TargetBookingId",
                table: "Notifications",
                column: "TargetBookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CustomerReviews_TargetCustomerReviewId",
                table: "Notifications",
                column: "TargetCustomerReviewId",
                principalTable: "CustomerReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Messages_TargetMessageId",
                table: "Notifications",
                column: "TargetMessageId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Rentals_TargetRentalId",
                table: "Notifications",
                column: "TargetRentalId",
                principalTable: "Rentals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_RenterReviews_TargetRenterReviewId",
                table: "Notifications",
                column: "TargetRenterReviewId",
                principalTable: "RenterReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AppUsers_TargetAppUserId",
                table: "Reports",
                column: "TargetAppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_CustomerReviews_TargetCustomerReviewId",
                table: "Reports",
                column: "TargetCustomerReviewId",
                principalTable: "CustomerReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Messages_TargetMessageId",
                table: "Reports",
                column: "TargetMessageId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_RenterReviews_TargetRenterReviewId",
                table: "Reports",
                column: "TargetRenterReviewId",
                principalTable: "RenterReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Vehicles_TargetVehicleId",
                table: "Reports",
                column: "TargetVehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Bookings_TargetBookingId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CustomerReviews_TargetCustomerReviewId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Messages_TargetMessageId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Rentals_TargetRentalId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_RenterReviews_TargetRenterReviewId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AppUsers_TargetAppUserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_CustomerReviews_TargetCustomerReviewId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Messages_TargetMessageId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_RenterReviews_TargetRenterReviewId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Vehicles_TargetVehicleId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TargetAppUserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TargetCustomerReviewId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TargetMessageId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TargetRenterReviewId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TargetVehicleId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TargetBookingId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TargetCustomerReviewId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TargetMessageId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TargetRentalId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TargetRenterReviewId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TargetAppUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TargetCustomerReviewId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TargetMessageId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TargetRenterReviewId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TargetBookingId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TargetCustomerReviewId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TargetMessageId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "TargetVehicleId",
                table: "Reports",
                newName: "TargetId");

            migrationBuilder.RenameColumn(
                name: "TargetRenterReviewId",
                table: "Notifications",
                newName: "TargetId");

            migrationBuilder.RenameColumn(
                name: "TargetRentalId",
                table: "Notifications",
                newName: "Target");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TargetId",
                table: "Reports",
                column: "TargetId",
                unique: true,
                filter: "[TargetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetId",
                table: "Notifications",
                column: "TargetId",
                unique: true,
                filter: "[TargetId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Bookings_TargetId",
                table: "Notifications",
                column: "TargetId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CustomerReviews_TargetId",
                table: "Notifications",
                column: "TargetId",
                principalTable: "CustomerReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Messages_TargetId",
                table: "Notifications",
                column: "TargetId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_RenterReviews_TargetId",
                table: "Notifications",
                column: "TargetId",
                principalTable: "RenterReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AppUsers_TargetId",
                table: "Reports",
                column: "TargetId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_CustomerReviews_TargetId",
                table: "Reports",
                column: "TargetId",
                principalTable: "CustomerReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Messages_TargetId",
                table: "Reports",
                column: "TargetId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_RenterReviews_TargetId",
                table: "Reports",
                column: "TargetId",
                principalTable: "RenterReviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Vehicles_TargetId",
                table: "Reports",
                column: "TargetId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }
    }
}
