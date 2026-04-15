using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderWarehouseAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up orphaned InstallationBookingId values in OrderItems
            migrationBuilder.Sql(
                @"UPDATE OrderItems 
                  SET InstallationBookingId = NULL 
                  WHERE InstallationBookingId IS NOT NULL 
                  AND NOT EXISTS (SELECT 1 FROM InstallationBookings WHERE Id = OrderItems.InstallationBookingId)");

            migrationBuilder.CreateTable(
                name: "OrderWarehouseAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    AllocatedQuantity = table.Column<int>(type: "int", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderWarehouseAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderWarehouseAllocations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_InstallationBookingId",
                table: "OrderItems",
                column: "InstallationBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderWarehouseAllocations_OrderItemId",
                table: "OrderWarehouseAllocations",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderWarehouseAllocations_WarehouseId",
                table: "OrderWarehouseAllocations",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_InstallationBookings_InstallationBookingId",
                table: "OrderItems",
                column: "InstallationBookingId",
                principalTable: "InstallationBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_InstallationBookings_InstallationBookingId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderWarehouseAllocations");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_InstallationBookingId",
                table: "OrderItems");
        }
    }
}
