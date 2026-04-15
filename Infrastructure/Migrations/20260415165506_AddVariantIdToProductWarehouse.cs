using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantIdToProductWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductWarehouses_ProductId_WarehouseId",
                table: "ProductWarehouses");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "ProductWarehouses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarehouses_ProductId_VariantId_WarehouseId",
                table: "ProductWarehouses",
                columns: new[] { "ProductId", "VariantId", "WarehouseId" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductWarehouses_ProductId_VariantId_WarehouseId",
                table: "ProductWarehouses");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ProductWarehouses");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarehouses_ProductId_WarehouseId",
                table: "ProductWarehouses",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);
        }
    }
}
