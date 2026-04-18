using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBasePriceFromProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_InstallationBookings_InstallationBookingId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockIssueDetails_StockIssues_StockIssueId",
                table: "StockIssueDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StockIssues_InstallationBookings_BookingId",
                table: "StockIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_StockIssues_Warehouses_WarehouseId",
                table: "StockIssues");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_InstallationBookingId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_InstallationBookingId",
                table: "OrderItems",
                column: "InstallationBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_InstallationBookings_InstallationBookingId",
                table: "OrderItems",
                column: "InstallationBookingId",
                principalTable: "InstallationBookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockIssueDetails_StockIssues_StockIssueId",
                table: "StockIssueDetails",
                column: "StockIssueId",
                principalTable: "StockIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockIssues_InstallationBookings_BookingId",
                table: "StockIssues",
                column: "BookingId",
                principalTable: "InstallationBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockIssues_Warehouses_WarehouseId",
                table: "StockIssues",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
