using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeSlotIdNullableInInstallationBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstallationBookings_SlotId",
                table: "InstallationBookings");

            migrationBuilder.AlterColumn<int>(
                name: "SlotId",
                table: "InstallationBookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_InstallationBookings_SlotId",
                table: "InstallationBookings",
                column: "SlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstallationBookings_SlotId",
                table: "InstallationBookings");

            migrationBuilder.AlterColumn<int>(
                name: "SlotId",
                table: "InstallationBookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstallationBookings_SlotId",
                table: "InstallationBookings",
                column: "SlotId",
                unique: true);
        }
    }
}
