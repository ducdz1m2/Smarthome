using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSerialNumberAndAddOrderItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_SerialNumber",
                table: "WarrantyRequests");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_SerialNumber",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_ProductId_VariantId_SerialNumber",
                table: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_SerialNumber",
                table: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SerialNumber",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "WarrantyRequests");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "OrderItems");

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "WarrantyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "WarrantyClaims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "Warranties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_OrderItemId",
                table: "WarrantyRequests",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_OrderItemId",
                table: "WarrantyClaims",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_OrderItemId",
                table: "Warranties",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_ProductId_VariantId_OrderItemId",
                table: "Warranties",
                columns: new[] { "ProductId", "VariantId", "OrderItemId" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_OrderItemId",
                table: "WarrantyRequests");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_OrderItemId",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_OrderItemId",
                table: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_ProductId_VariantId_OrderItemId",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "WarrantyRequests");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Warranties");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "WarrantyRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Warranties",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "OrderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_SerialNumber",
                table: "WarrantyRequests",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_SerialNumber",
                table: "WarrantyClaims",
                column: "SerialNumber");

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
                name: "IX_OrderItems_SerialNumber",
                table: "OrderItems",
                column: "SerialNumber");
        }
    }
}
