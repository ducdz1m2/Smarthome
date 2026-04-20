using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductRatingToUseOrderItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductRatings_ProductId_VariantId_SerialNumber_CustomerId",
                table: "ProductRatings");

            migrationBuilder.DropIndex(
                name: "IX_ProductRatings_SerialNumber",
                table: "ProductRatings");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "ProductRatings");

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "ProductRatings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_OrderItemId",
                table: "ProductRatings",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatings_ProductId_VariantId_OrderItemId_CustomerId",
                table: "ProductRatings",
                columns: new[] { "ProductId", "VariantId", "OrderItemId", "CustomerId" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductRatings_OrderItemId",
                table: "ProductRatings");

            migrationBuilder.DropIndex(
                name: "IX_ProductRatings_ProductId_VariantId_OrderItemId_CustomerId",
                table: "ProductRatings");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "ProductRatings");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "ProductRatings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

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
        }
    }
}
