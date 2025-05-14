using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationRelationsToManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Notifications_NotificationId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_NotificationId",
                table: "Review");

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
                name: "NotificationId",
                table: "Review");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetBookingId",
                table: "Notifications",
                column: "TargetBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetCustomerReviewId",
                table: "Notifications",
                column: "TargetCustomerReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetMessageId",
                table: "Notifications",
                column: "TargetMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetRentalId",
                table: "Notifications",
                column: "TargetRentalId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetRenterReviewId",
                table: "Notifications",
                column: "TargetRenterReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "Review",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Review_NotificationId",
                table: "Review",
                column: "NotificationId");

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
                name: "FK_Review_Notifications_NotificationId",
                table: "Review",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id");
        }
    }
}
