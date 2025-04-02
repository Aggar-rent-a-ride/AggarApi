using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPrecisionOfRatesInReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Punctuality",
                table: "Review",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Behavior",
                table: "Review",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Punctuality",
                table: "RenterReviews",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Care",
                table: "RenterReviews",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Behavior",
                table: "RenterReviews",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Truthfulness",
                table: "CustomerReviews",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Punctuality",
                table: "CustomerReviews",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Behavior",
                table: "CustomerReviews",
                type: "float(1)",
                precision: 1,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Punctuality",
                table: "Review",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Behavior",
                table: "Review",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Punctuality",
                table: "RenterReviews",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Care",
                table: "RenterReviews",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Behavior",
                table: "RenterReviews",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Truthfulness",
                table: "CustomerReviews",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Punctuality",
                table: "CustomerReviews",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);

            migrationBuilder.AlterColumn<double>(
                name: "Behavior",
                table: "CustomerReviews",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(1)",
                oldPrecision: 1,
                oldScale: 1);
        }
    }
}
