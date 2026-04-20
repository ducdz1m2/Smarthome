using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWarrantyToProductBased : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyRequestItems");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_OrderItemId",
                table: "Warranties");

            // Make OrderId nullable first to handle existing data
            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "WarrantyRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Set existing OrderId values to null since they don't correspond to Warranty.Id
            migrationBuilder.Sql("UPDATE WarrantyRequests SET OrderId = NULL");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "WarrantyRequests",
                newName: "WarrantyId");

            migrationBuilder.RenameIndex(
                name: "IX_WarrantyRequests_OrderId",
                table: "WarrantyRequests",
                newName: "IX_WarrantyRequests_WarrantyId");

            migrationBuilder.RenameColumn(
                name: "OrderItemId",
                table: "Warranties",
                newName: "DurationMonths");

            migrationBuilder.AddColumn<int>(
                name: "InstallationBookingId",
                table: "WarrantyRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "WarrantyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "WarrantyRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "WarrantyRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "WarrantyClaims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "WarrantyClaims",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyRequestId",
                table: "WarrantyClaims",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClaimsCount",
                table: "Warranties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Warranties",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "Warranties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRatings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_InstallationBookingId",
                table: "WarrantyRequests",
                column: "InstallationBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_ProductId",
                table: "WarrantyRequests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_SerialNumber",
                table: "WarrantyRequests",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_ProductId",
                table: "WarrantyClaims",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_SerialNumber",
                table: "WarrantyClaims",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_WarrantyRequestId",
                table: "WarrantyClaims",
                column: "WarrantyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_ProductId_VariantId_SerialNumber",
                table: "Warranties",
                columns: new[] { "ProductId", "VariantId", "SerialNumber" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_SerialNumber",
                table: "Warranties",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_CustomerId",
                table: "ProductRatings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_ProductId",
                table: "ProductRatings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_ProductId_VariantId_SerialNumber_CustomerId",
                table: "ProductRatings",
                columns: new[] { "ProductId", "VariantId", "SerialNumber", "CustomerId" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_SerialNumber",
                table: "ProductRatings",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_Status",
                table: "ProductRatings",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyRequests_Warranties_WarrantyId",
                table: "WarrantyRequests",
                column: "WarrantyId",
                principalTable: "Warranties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyRequests_Warranties_WarrantyId",
                table: "WarrantyRequests");

            migrationBuilder.DropTable(
                name: "ProductRatings");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_InstallationBookingId",
                table: "WarrantyRequests");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_ProductId",
                table: "WarrantyRequests");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_SerialNumber",
                table: "WarrantyRequests");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_ProductId",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_SerialNumber",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_WarrantyRequestId",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_ProductId_VariantId_SerialNumber",
                table: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_SerialNumber",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "InstallationBookingId",
                table: "WarrantyRequests");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "WarrantyRequests");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "WarrantyRequests");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "WarrantyRequests");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "WarrantyRequestId",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "ClaimsCount",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "Warranties");

            migrationBuilder.RenameColumn(
                name: "WarrantyId",
                table: "WarrantyRequests",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WarrantyRequests_WarrantyId",
                table: "WarrantyRequests",
                newName: "IX_WarrantyRequests_OrderId");

            // Make OrderId not nullable again
            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "WarrantyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "DurationMonths",
                table: "Warranties",
                newName: "OrderItemId");

            migrationBuilder.CreateTable(
                name: "WarrantyRequestItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDamaged = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReturnedToInventory = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyRequestItems_WarrantyRequests_WarrantyRequestId",
                        column: x => x.WarrantyRequestId,
                        principalTable: "WarrantyRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_OrderItemId",
                table: "Warranties",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequestItems_OrderItemId",
                table: "WarrantyRequestItems",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequestItems_WarrantyRequestId",
                table: "WarrantyRequestItems",
                column: "WarrantyRequestId");
        }
    }
}
